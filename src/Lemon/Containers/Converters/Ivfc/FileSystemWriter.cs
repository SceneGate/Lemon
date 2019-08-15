// Copyright (c) 2019 SceneGate
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <http://www.gnu.org/licenses/>.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Lemon.Logging;
using Yarhl.FileSystem;
using Yarhl.IO;

namespace Lemon.Containers.Converters.Ivfc
{
    internal class FileSystemWriter
    {
        const int HeaderSize = 0x28;
        const int DirMetadataSize = 0x18;
        const int FileMetadataSize = 0x20;
        const int HashSize = 4;
        static readonly ILog Logger = LogProvider.GetCurrentClassLogger();
        static readonly Encoding NameEncoding = Encoding.Unicode;

        readonly Node root;

        FileSystemInfo info;
        Section dirHashTable;
        Section dirInfoTable;
        Section fileHashTable;
        Section fileInfoTable;
        Section fileData;
        List<Node> files;

        DataWriter streamWriter;
        DataWriter writer;
        DataReader reader;

        public FileSystemWriter(Node root)
        {
            this.root = root;

            Analyze();
            PrecalculateSize();
        }

        public long Size => fileData.Offset + fileData.Size;

        public long MetadataSize {
            get {
                return dirHashTable.Size + dirInfoTable.Size
                    + fileHashTable.Size + fileInfoTable.Size;
            }
        }

        void Analyze()
        {
            info = new FileSystemInfo();
            info.Directories = 1; // root, no name.

            foreach (var node in Navigator.IterateNodes(root, NavigationMode.BreadthFirst)) {
                if (node.IsContainer) {
                    info.Directories++;
                    info.DirNamesLength += NameEncoding.GetByteCount(node.Name).Pad(0x04);
                } else if (node.Stream == null) {
                    Logger.Error($"Node '{node.Path}' is not a folder or a binary file.");
                } else {
                    info.Files++;
                    info.FileNamesLength += NameEncoding.GetByteCount(node.Name).Pad(0x04);
                    info.FileDataLength += node.Stream.Length.Pad(0x10);
                }
            }

            info.DirectoryHashes = NameHash.CalculateTableLength(info.Directories);
            info.FileHashes = NameHash.CalculateTableLength(info.Files);
        }

        void PrecalculateSize()
        {
            dirHashTable = new Section {
                Offset = HeaderSize,
                Size = HashSize * info.DirectoryHashes,
            };

            dirInfoTable = new Section {
                Offset = dirHashTable.Offset + dirHashTable.Size,
                Size = (DirMetadataSize * info.Directories) + info.DirNamesLength,
            };

            fileHashTable = new Section {
                Offset = dirInfoTable.Offset + dirInfoTable.Size,
                Size = HashSize * info.FileHashes,
            };

            fileInfoTable = new Section {
                Offset = fileHashTable.Offset + fileHashTable.Size,
                Size = (FileMetadataSize * info.Files) + info.FileNamesLength,
            };

            fileData = new Section {
                Offset = (fileInfoTable.Offset + fileInfoTable.Size).Pad(0x10),
                Size = info.FileDataLength,
            };
        }

        public void Write(DataStream stream)
        {
            dirHashTable.ResetPosition();
            dirInfoTable.ResetPosition();
            fileHashTable.ResetPosition();
            fileInfoTable.ResetPosition();
            fileData.ResetPosition();
            files = new List<Node>();

            streamWriter = new DataWriter(stream);
            using (var metadataStream = new DataStream())
            {
                writer = new DataWriter(metadataStream);
                reader = new DataReader(metadataStream);

                WriteHeader();

                // Prefill so we can jump between sections
                // Also, unused fields must be at 0xFF
                writer.WriteTimes(0xFF, MetadataSize);

                WriteDirectoryInfo(root, false, true, (uint)dirInfoTable.Offset);
                WriteNode(root, (uint)dirInfoTable.Offset);

                metadataStream.WriteTo(stream);
                streamWriter.WritePadding(0x00, 0x10);
            }

            WriteFiles();
        }

        void WriteFiles()
        {
            foreach (var file in files)
            {
                file.Stream.WriteTo(streamWriter.Stream);
                streamWriter.WritePadding(0x00, 0x10);
            }
        }

        void WriteNode(Node current, uint currentOffset)
        {
            var files = current.Children.Where(n => !n.IsContainer).ToList();
            for (int i = 0; i < files.Count; i++)
            {
                WriteFileInfo(files[i], i == 0, i + 1 == files.Count, currentOffset);
            }

            var dirs = current.Children.Where(n => n.IsContainer).ToList();
            uint[] offsets = new uint[dirs.Count];
            for (int i = 0; i < dirs.Count; i++)
            {
                offsets[i] = (uint)dirInfoTable.Position;
                WriteDirectoryInfo(dirs[i], i == 0, i + 1 == dirs.Count, currentOffset);
            }

            for (int i = 0; i < dirs.Count; i++)
            {
                WriteNode(dirs[i], offsets[i]);
            }
        }

        void WriteHeader()
        {
            writer.Write(HeaderSize);
            writer.Write((uint)dirHashTable.Offset);
            writer.Write((uint)dirHashTable.Size);
            writer.Write((uint)dirInfoTable.Offset);
            writer.Write((uint)dirInfoTable.Size);
            writer.Write((uint)fileHashTable.Offset);
            writer.Write((uint)fileHashTable.Size);
            writer.Write((uint)fileInfoTable.Offset);
            writer.Write((uint)fileInfoTable.Size);
            writer.Write((uint)fileData.Offset);
        }

        void WriteDirectoryInfo(Node dir, bool isFirst, bool isLast, uint parentOffset)
        {
            uint currentPosition = (uint)dirInfoTable.Position;
            uint relativeParent = (uint)(parentOffset - dirInfoTable.Offset);
            if (isFirst) {
                writer.Stream.Position = parentOffset + 0x08;
                writer.Write((uint)dirInfoTable.RelativePosition);
            }

            // Calculate the hash from the name and the parent offset.
            // The root node doesn't have name.
            string dirName = dir.Parent == null ? string.Empty : dir.Name;
            byte[] name = NameEncoding.GetBytes(dirName);
            uint hash = NameHash.CalculateHash(relativeParent, name);
            int hashIdx = (int)(hash % info.DirectoryHashes);

            // Get the previous hash and overwrite with our hash
            int hashOffset = (int)dirHashTable.Offset + (HashSize * hashIdx);
            reader.Stream.Position = hashOffset;
            uint collisionOffset = reader.ReadUInt32();

            writer.Stream.Position = hashOffset;
            writer.Write((uint)dirInfoTable.RelativePosition);

            // Write our info
            writer.Stream.Position = dirInfoTable.Position;
            writer.Write(relativeParent);
            writer.Write(0xFFFFFFFF); // next sibling later we may refill it
            writer.Write(0xFFFFFFFF); // firsth child later we may refill it
            writer.Write(0xFFFFFFFF); // first file later we may refill it
            writer.Write(collisionOffset);
            writer.Write(name.Length);
            writer.Write(name);
            writer.WritePadding(0x00, 4);

            dirInfoTable.Position = (uint)writer.Stream.Position;

            // If there are siblings, update our offset pointing to the next one.
            if (!isLast) {
                writer.Stream.Position = currentPosition + 0x04;
                writer.Write((uint)dirInfoTable.RelativePosition);
            }
        }

        void WriteFileInfo(Node file, bool isFirst, bool isLast, uint parentOffset)
        {
            uint currentPosition = (uint)fileInfoTable.Position;
            uint relativeParent = (uint)(parentOffset - dirInfoTable.Offset);
            if (isFirst) {
                writer.Stream.Position = parentOffset + 0x0C;
                writer.Write((uint)fileInfoTable.RelativePosition);
            }

            // Calculate the hash from the name and the parent offset.
            byte[] name = NameEncoding.GetBytes(file.Name);
            uint hash = NameHash.CalculateHash(relativeParent, name);
            int hashIdx = (int)(hash % info.FileHashes);

            // Get the previous hash and overwrite with our hash
            int hashOffset = (int)fileHashTable.Offset + (HashSize * hashIdx);
            reader.Stream.Position = hashOffset;
            uint collisionOffset = reader.ReadUInt32();

            writer.Stream.Position = hashOffset;
            writer.Write((uint)fileInfoTable.RelativePosition);

            // Write our info
            writer.Stream.Position = fileInfoTable.Position;
            writer.Write(relativeParent);
            writer.Write(0xFFFFFFFF); // later we may refill it
            writer.Write(fileData.RelativePosition);
            writer.Write(file.Stream.Length);
            writer.Write(collisionOffset);
            writer.Write(name.Length);
            writer.Write(name);
            writer.WritePadding(0x00, 4);

            fileInfoTable.Position = writer.Stream.Position;

            // If there are siblings, update our offset pointing to the next one.
            if (!isLast) {
                writer.Stream.Position = currentPosition + 0x04;
                writer.Write((uint)fileInfoTable.RelativePosition);
            }

            files.Add(file);
            fileData.Position += file.Stream.Length.Pad(0x10);
        }

        class FileSystemInfo
        {
            public int Directories { get; set; }

            public int DirNamesLength { get; set; }

            public int DirectoryHashes { get; set; }

            public int Files { get; set; }

            public int FileNamesLength { get; set; }

            public int FileHashes { get; set; }

            public long FileDataLength { get; set; }
        }

        class Section
        {
            public long Offset { get; set; }

            public long Position { get; set; }

            public long RelativePosition => Position - Offset;

            public long Size { get; set; }

            public void ResetPosition()
            {
                Position = Offset;
            }
        }
    }
}
