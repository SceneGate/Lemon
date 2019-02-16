// Binary2Ncsd.cs
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
    using System.Linq;
    using Lemon.Containers.Formats;
    using Yarhl.FileFormat;
    using Yarhl.FileSystem;
    using Yarhl.IO;

    /// <summary>
    /// Converter for Binary streams into a NCSD instance.
    /// </summary>
    public class Binary2Ncsd : IConverter<BinaryFormat, Ncsd>
    {
        /// <summary>
        /// Gets the name of a partition from its index.
        /// </summary>
        /// <param name="index">Index of the partition.</param>
        /// <returns>The associated partition's name.</returns>
        public static string GetPartitionName(int index)
        {
            switch (index) {
                case 0:
                    return "program";
                case 1:
                    return "manual";
                case 2:
                    return "download_play";
                case 6:
                    return "new3ds_update";
                case 7:
                    return "update";
                default:
                    throw new FormatException("Unsupported partition");
            }
        }

        /// <summary>
        /// Converts a binary stream into a NCSD instance.
        /// </summary>
        /// <param name="source">Binary stream to convert.</param>
        /// <returns>The new NCSD instance.</returns>
        public Ncsd Convert(BinaryFormat source)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            var ncsd = new Ncsd();
            var reader = new DataReader(source.Stream);

            // First read the header
            var header = ncsd.Header;
            header.Signature = reader.ReadBytes(0x100);
            if (reader.ReadString(4) != NcsdHeader.MagicId)
                throw new FormatException("Invalid Magic ID");

            header.Size = reader.ReadUInt32() * NcsdHeader.Unit;
            header.MediaId = reader.ReadUInt64();
            header.FirmwaresType = reader.ReadBytes(Ncsd.NumPartitions)
                .Select(x => (FirmwareType)x)
                .ToArray();
            header.CryptType = reader.ReadBytes(Ncsd.NumPartitions);

            // Now add the subfiles / partitions
            for (int i = 0; i < Ncsd.NumPartitions; i++) {
                long offset = reader.ReadUInt32() * NcsdHeader.Unit;
                long size = reader.ReadUInt32() * NcsdHeader.Unit;
                if (size == 0) {
                    continue;
                }

                string name = (header.FirmwaresType[i] == FirmwareType.None)
                    ? GetPartitionName(i)
                    : $"firm{i}";
                var childBinary = new BinaryFormat(source.Stream, offset, size);
                var child = new Node(name, childBinary);
                ncsd.Root.Add(child);
            }

            // TODO: Read rest of header
            return ncsd;
        }
    }
}
