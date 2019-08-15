// BinaryExeFs2NodeContainer.cs
//
// Copyright (c) 2019 SceneGate Team
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
namespace Lemon.Containers.Converters
{
    using System;
    using System.Linq;
    using System.Security.Cryptography;
    using System.Text;
    using Lemon.Logging;
    using Yarhl.FileFormat;
    using Yarhl.FileSystem;
    using Yarhl.IO;

    /// <summary>
    /// Converter for Binary streams into a file system following the
    /// Executable File System tree format.
    /// </summary>
    public class BinaryExeFs2NodeContainer :
        IConverter<BinaryFormat, NodeContainerFormat>,
        IConverter<NodeContainerFormat, BinaryFormat>
    {
        const int NumberFiles = 10;
        const int HeaderSize = 0x200;
        const int Sha256Size = 0x20;

        static readonly ILog Logger = LogProvider.GetCurrentClassLogger();

        /// <summary>
        /// Converts a binary stream with Executable file system into nodes.
        /// </summary>
        /// <param name="source">The binary stream to convert.</param>
        /// <returns>The file system from the ExeFS stream.</returns>
        public NodeContainerFormat Convert(BinaryFormat source)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            var reader = new DataReader(source.Stream) {
                DefaultEncoding = Encoding.ASCII,
            };
            var container = new NodeContainerFormat();

            for (int i = 0; i < NumberFiles; i++) {
                Node child = GetNodeFromHeader(reader);
                if (child != null) {
                    container.Root.Add(child);
                }
            }

            reader.SkipPadding(0x40);

            // Validate the hashes
            int totalHashSize = NumberFiles * Sha256Size;
            for (int i = 0; i < container.Root.Children.Count; i++) {
                // Hash are stored in reverse order, from end to beginning
                byte[] expected = null;
                source.Stream.RunInPosition(
                    () => expected = reader.ReadBytes(Sha256Size),
                    totalHashSize - (Sha256Size * (i + 1)),
                    SeekMode.Current);

                byte[] actual = ComputeHash(container.Root.Children[i].Stream);

                if (!expected.SequenceEqual(actual)) {
                    Logger.Warn($"Wrong hash for child {container.Root.Children[i].Name}");
                }
            }

            return container;
        }

        /// <summary>
        /// Converts a container of nodes into a binary of format
        /// Executable file system.
        /// </summary>
        /// <param name="source">Container of nodes.</param>
        /// <returns>The new binary container.</returns>
        public BinaryFormat Convert(NodeContainerFormat source)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            var binary = new BinaryFormat();
            var writer = new DataWriter(binary.Stream) {
                DefaultEncoding = Encoding.ASCII,
            };

            // Generate empty header so we can write content at the same time
            writer.WriteTimes(0x00, HeaderSize);

            // Write table with files
            binary.Stream.Position = 0;
            foreach (var child in source.Root.Children) {
                writer.Write(child.Name, 8);
                writer.Write((uint)(binary.Stream.Length - HeaderSize));
                writer.Write((uint)child.Stream.Length);

                binary.Stream.RunInPosition(
                    () => {
                        child.Stream.WriteTo(binary.Stream);
                        writer.WritePadding(0x00, 0x200);
                    },
                    0,
                    SeekMode.End);
            }

            binary.Stream.Position = NumberFiles * 0x10;
            writer.WritePadding(0x00, 0x40);

            int totalHashSize = NumberFiles * Sha256Size;
            for (int i = 0; i < source.Root.Children.Count; i++) {
                byte[] hash = ComputeHash(source.Root.Children[i].Stream);

                // Hash are stored in reverse order, from end to beginning
                binary.Stream.RunInPosition(
                    () => writer.Write(hash),
                    totalHashSize - (Sha256Size * (i + 1)),
                    SeekMode.Current);
            }

            return binary;
        }

        static Node GetNodeFromHeader(DataReader reader)
        {
            string name = reader.ReadString(8).Replace("\0", string.Empty);
            uint offset = reader.ReadUInt32() + HeaderSize;
            uint size = reader.ReadUInt32();

            Node node = null;
            if (!string.IsNullOrEmpty(name)) {
                node = new Node(name, new BinaryFormat(reader.Stream, offset, size));
            }

            return node;
        }

        static byte[] ComputeHash(DataStream stream)
        {
            byte[] hash;
            using (SHA256 sha = SHA256.Create()) {
                // Read the file in blocks of 64 KB (small enough for the SOH).
                stream.Position = 0;
                int offset = 0;
                byte[] buffer = new byte[1024 * 64];
                while (offset + buffer.Length < stream.Length) {
                    stream.Read(buffer, 0, buffer.Length);
                    offset += sha.TransformBlock(buffer, 0, buffer.Length, buffer, 0);
                }

                int finalSize = (int)(stream.Length - offset);
                stream.Read(buffer, 0, finalSize);
                sha.TransformFinalBlock(buffer, 0, finalSize);

                hash = sha.Hash;
            }

            return hash;
        }
    }
}
