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
    public class BinaryExeFs2NodeContainer : IConverter<BinaryFormat, NodeContainerFormat>
    {
        const int NumberFiles = 10;
        const int HeaderSize = 0x200;
        const int ReservedSize = 0x20;

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

            byte[] reserved = reader.ReadBytes(ReservedSize);
            if (reserved.Any(x => x != 0x00)) {
                Logger.Warn("Expected 'Reserved' to be empty but it has content.");
            }

            // Validate the hashes
            const int Sha256Size = 0x20;
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
