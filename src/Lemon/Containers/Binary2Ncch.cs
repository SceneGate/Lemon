// Binary2Ncch.cs
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
    using Yarhl.FileFormat;
    using Yarhl.FileSystem;
    using Yarhl.IO;

    public class Binary2Ncch : IConverter<BinaryFormat, Ncch>
    {
        public Ncch Convert(BinaryFormat source)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            var reader = new DataReader(source.Stream);

            var ncch = new Ncch();
            ncch.Header.Signature = reader.ReadBytes(0x100);

            if (reader.ReadString(4) != NcchHeader.MagicId)
                throw new FormatException("Invalid Magic ID");

            // TODO: Rest rest of header

            source.Stream.Position = 0x190;
            ncch.Root.Add(new Node("sdk_info.txt", ReadBinaryChild(reader)));
            ncch.Root.Add(new Node("logo.bin", ReadBinaryChild(reader)));
            ncch.Root.Add(new Node("system.fs", ReadBinaryChild(reader)));
            source.Stream.Position += 8; // TODO

            var rom = new Node("rom", ReadBinaryChild(reader));
            rom.Transform<BinaryIvfc2NodeContainer, BinaryFormat, NodeContainerFormat>();
            ncch.Root.Add(rom);

            // TODO: Read rest of header

            return ncch;
        }

        static BinaryFormat ReadBinaryChild(DataReader reader)
        {
            long offset = reader.ReadUInt32() * NcchHeader.Unit;
            long size = reader.ReadUInt32() * NcchHeader.Unit;
            return new BinaryFormat(reader.Stream, offset, size);
        }
    }
}
