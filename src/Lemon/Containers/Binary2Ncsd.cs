// Binary2Ncsd.cs
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

    public class Binary2Ncsd : IConverter<BinaryFormat, Ncsd>
    {
        public Ncsd Convert(BinaryFormat source)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            var reader = new DataReader(source.Stream);

            var ncsd = new Ncsd();
            ncsd.Header.Signature = reader.ReadBytes(0x100);

            if (reader.ReadString(4) != NcsdHeader.MagicId)
                throw new FormatException("Invalid Magic ID");

            ncsd.Header.Size = reader.ReadUInt32() * NcsdHeader.Unit;
            ncsd.Header.MediaId = reader.ReadUInt64();
            reader.Stream.Position += 8;
            // ncsd.Header.FileSystemType = reader.ReadBytes(Ncsd.NumPartitions)
            //     .Cast<NcsdFileSystemType>()
            //     .ToArray();
            ncsd.Header.CryptType = reader.ReadBytes(Ncsd.NumPartitions);

            for (int i = 0; i < Ncsd.NumPartitions; i++) {
                long offset = reader.ReadUInt32() * NcsdHeader.Unit;
                long size = reader.ReadUInt32() * NcsdHeader.Unit;

                var childBinary = new BinaryFormat(source.Stream, offset, size);
                var child = new Node(GetPartitionName(i), childBinary);
                if (i == 0) {
                    child.Transform<Binary2Ncch, BinaryFormat, Ncch>();
                }

                ncsd.Root.Add(child);
            }

            // TODO: Read rest of header

            return ncsd;
        }

        static string GetPartitionName(int index)
        {
            switch (index) {
                case 0:
                    return "program";
                case 1:
                    return "manual.cfa";
                case 2:
                    return "download_play.cfa";
                case 6:
                    return "new3ds_update.cfa";
                case 7:
                    return "update.cfa";
                default:
                    return $"partition{index}.bin";
            }
        }
    }
}
