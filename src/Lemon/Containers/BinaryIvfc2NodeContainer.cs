// BinaryIvfc2NodeContainer.cs
//
// Author:
//      Benito Palacios Sánchez (aka pleonex) <benito356@gmail.com>
//
// Copyright (c) 2019 Benito Palacios Sánchez
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
namespace Lemon.Containers
{
    using System;
    using System.Linq;
    using System.Text;
    using Yarhl.FileFormat;
    using Yarhl.FileSystem;
    using Yarhl.IO;

    public class BinaryIvfc2NodeContainer : IConverter<BinaryFormat, NodeContainerFormat>
    {
        uint dirInfoOffset;
        uint fileInfoOffset;
        uint fileDataOffset;

        public static string MagicId { get { return "IVFC"; } }

        public static uint Version { get { return 0x0001_0000; } }

        public NodeContainerFormat Convert(BinaryFormat source)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            var reader = new DataReader(source.Stream);
            var root = new NodeContainerFormat();

            if (reader.ReadString(4) != MagicId)
                throw new FormatException("Invalid Magic ID");
            if (reader.ReadUInt32() != Version)
                throw new FormatException("Invalid version");

            // TODO: Figure out what to do with Level 1 and Level 2
            reader.Stream.Position = 0x1000; // TODO: Calculate

            // Level 3
            // skip to directory metadata info
            reader.Stream.Position += 0x0C;
            dirInfoOffset = reader.ReadUInt32() + 0x1000;

            // skip to file metadata info
            reader.Stream.Position += 0x0C;
            fileInfoOffset = reader.ReadUInt32() + 0x1000;
            reader.ReadUInt32(); // size

            fileDataOffset = reader.ReadUInt32() + 0x1000;

            reader.Stream.Position = dirInfoOffset;
            ReadDirectoryInfo(root.Root, reader);

            return root;
        }

        void ReadDirectoryInfo(Node current, DataReader reader)
        {
            reader.ReadUInt32();
            int siblingDir = reader.ReadInt32();
            int subDir = reader.ReadInt32();
            int subFile = reader.ReadInt32();
            reader.ReadUInt32();

            int nameLength = reader.ReadInt32();
            Node myNode;
            if (nameLength == 0) {
                myNode = current;
            } else {
                string name = Encoding.Unicode.GetString(reader.ReadBytes(nameLength));
                myNode = NodeFactory.CreateContainer(name);
                current.Add(myNode);
            }

            if (subFile != -1) {
                reader.Stream.Position = fileInfoOffset + subFile;
                ReadFileInfo(myNode, reader);
            }

            if (siblingDir != -1) {
                reader.Stream.Position = dirInfoOffset + siblingDir;
                ReadDirectoryInfo(current, reader);
            }

            if (subDir != -1) {
                reader.Stream.Position = dirInfoOffset + subDir;
                ReadDirectoryInfo(myNode, reader);
            }
        }

        void ReadFileInfo(Node current, DataReader reader)
        {
            reader.ReadUInt32();
            int siblingFile = reader.ReadInt32();
            long offset = reader.ReadInt64();
            long size = reader.ReadInt64();
            reader.ReadUInt32();

            int nameLength = reader.ReadInt32();
            string name = Encoding.Unicode.GetString(reader.ReadBytes(nameLength));
            
            var binary = new BinaryFormat(reader.Stream, fileDataOffset + offset, size);
            current.Add(new Node(name, binary));

            if (siblingFile != -1) {
                reader.Stream.Position = fileInfoOffset + siblingFile;
                ReadFileInfo(current, reader);
            }
        }
    }
}
