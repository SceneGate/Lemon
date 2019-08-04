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
    using System.Text;
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

            // TODO: Validate hashes
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
    }
}
