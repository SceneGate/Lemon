// Binary2Ncch.cs
//
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
namespace Lemon.Containers.Converters
{
    using System;
    using Lemon.Containers.Formats;
    using Yarhl.FileFormat;
    using Yarhl.FileSystem;
    using Yarhl.IO;

    /// <summary>
    /// Converter for Binary streams into a NCCH instance.
    /// </summary>
    public class Binary2Ncch : IConverter<BinaryFormat, Ncch>
    {
        /// <summary>
        /// Converts a binary stream into a NCCH instance.
        /// </summary>
        /// <param name="source">Binary stream to convert.</param>
        /// <returns>The new NCCH instance.</returns>
        public Ncch Convert(BinaryFormat source)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            var ncch = new Ncch();
            var reader = new DataReader(source.Stream);

            // First read the header
            var header = ncch.Header;
            header.Signature = reader.ReadBytes(0x100);
            if (reader.ReadString(4) != NcchHeader.MagicId)
                throw new FormatException("Invalid Magic ID");

            // TODO: Read header
            source.Stream.Position = 0x190;

            // Read the subfiles
            AddChildIfExists("sdk_info.txt", ncch.Root, reader);
            AddChildIfExists("logo.bin", ncch.Root, reader);
            AddChildIfExists("system", ncch.Root, reader);

            // TODO: Read these fields
            source.Stream.Position += 8;
            AddChildIfExists("rom", ncch.Root, reader);

            // TODO: Read rest of header
            return ncch;
        }

        static void AddChildIfExists(string name, Node root, DataReader reader)
        {
            BinaryFormat binary = ReadBinaryChild(reader);
            if (binary != null) {
                root.Add(new Node(name, binary));
            }
        }

        static BinaryFormat ReadBinaryChild(DataReader reader)
        {
            long offset = reader.ReadUInt32() * NcchHeader.Unit;
            long size = reader.ReadUInt32() * NcchHeader.Unit;
            if (size == 0) {
                return null;
            }

            return new BinaryFormat(reader.Stream, offset, size);
        }
    }
}
