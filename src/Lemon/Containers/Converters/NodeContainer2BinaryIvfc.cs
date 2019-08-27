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
    using Lemon.Containers.Converters.Ivfc;
    using Yarhl.FileFormat;
    using Yarhl.FileSystem;
    using Yarhl.IO;

    /// <summary>
    /// Converter for Binary streams into a file system following the
    /// IVFC tree format.
    /// </summary>
    /// <remarks>
    /// <para>The binary IVFC format consists in the following sections:</para>
    /// * IVFC Header
    /// * Level 0
    /// * Level 3
    /// |--* File system header
    /// |--* Directories hash
    /// |--* Directories info
    /// |--* Files hash
    /// |--* Files info
    /// |--* File data
    /// * Level 1
    /// * Level 2.
    /// <para>Level 0, 1 and 2 only contain SHA-256 hashes of the upper layer.
    /// Level 3 contains the file system metadata and file data.</para>
    /// </remarks>
    public class NodeContainer2BinaryIvfc :
        IInitializer<DataStream>,
        IConverter<NodeContainerFormat, BinaryFormat>
    {
        const int BlockSizeLog = 0x0C;
        const int BlockSize = 1 << BlockSizeLog;

        DataStream stream;

        /// <summary>
        /// Gets the magic identifier of the format.
        /// </summary>
        /// <value>The magic ID of the format.</value>
        public static string MagicId {
            get { return "IVFC"; }
        }

        /// <summary>
        /// Gets the format version.
        /// </summary>
        /// <value>The format version.</value>
        public static uint Version {
            get { return 0x0001_0000; }
        }

        /// <summary>
        /// Initialize the converter by providing the stream to write to.
        /// </summary>
        /// <param name="stream">Stream to write to.</param>
        public void Initialize(DataStream stream)
        {
            this.stream = stream;
        }

        /// <summary>
        /// Converts a file system into a memory binary stream with IVFC format.
        /// </summary>
        /// <param name="source">The node file system to convert.</param>
        /// <returns>The memory binary stream with IVFC format.</returns>
        public BinaryFormat Convert(NodeContainerFormat source)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            var binary = (stream != null) ? new BinaryFormat(stream) : new BinaryFormat();
            var writer = new DataWriter(binary.Stream);

            // Analyze the file system to pre-calculate the sizes.
            // As said, Level 3 contains the whole file system,
            // the other level just contain SHA-256 hashes of every block
            // from the lower layer. So we get the number of blocks
            // (number of hashes) and multiple by the hash size.
            var fsWriter = new FileSystemWriter(source.Root);

            const int LevelHashSize = 0x20; // SHA-256 size
            long[] levelSizes = new long[4];
            levelSizes[3] = fsWriter.Size;
            levelSizes[2] = (levelSizes[3].Pad(BlockSize) / BlockSize) * LevelHashSize;
            levelSizes[1] = (levelSizes[2].Pad(BlockSize) / BlockSize) * LevelHashSize;
            levelSizes[0] = (levelSizes[1].Pad(BlockSize) / BlockSize) * LevelHashSize;

            WriteHeader(writer, levelSizes);
            writer.WritePadding(0x00, 0x10);

            long level0DataOffset = binary.Stream.Position;
            writer.WriteTimes(0x00, levelSizes[0]); // pre-allocate
            writer.WritePadding(0x00, BlockSize);
            long level3Offset = binary.Stream.Position;

            // Increase the base length so we can create a substream of the correct size
            // This operation doesn't write and it returns almost inmediatly,
            // the write happens on the first byte written on the new file space.
            long level3Padded = levelSizes[3].Pad(BlockSize);
            binary.Stream.BaseStream.SetLength(binary.Stream.BaseStream.Length + level3Padded);
            binary.Stream.Length += level3Padded;

            // Create "special" data streams that will create hashes on-the-fly.
            var level1 = new LevelStream(BlockSize);
            var level2 = new LevelStream(BlockSize);
            var level3 = new LevelStream(BlockSize, binary.Stream.BaseStream);
            using (var level0Stream = new DataStream(binary.Stream, level0DataOffset, levelSizes[0]))
            using (var level1Stream = new DataStream(level1))
            using (var level2Stream = new DataStream(level2))
            using (var level3Stream = new DataStream(level3, level3Offset, level3Padded)) {
                level3.BlockWritten += (_, e) => level2Stream.Write(e.Hash, 0, e.Hash.Length);
                level2.BlockWritten += (_, e) => level1Stream.Write(e.Hash, 0, e.Hash.Length);
                level1.BlockWritten += (_, e) => level0Stream.Write(e.Hash, 0, e.Hash.Length);

                fsWriter.Write(level3Stream);

                // Pad remaining block size in order.
                new DataWriter(level3Stream).WritePadding(0x00, BlockSize);
                new DataWriter(level2Stream).WritePadding(0x00, BlockSize);
                new DataWriter(level1Stream).WritePadding(0x00, BlockSize);

                // Write streams in order.
                binary.Stream.Position = binary.Stream.Length;
                level1Stream.WriteTo(binary.Stream);
                level2Stream.WriteTo(binary.Stream);
            }

            return binary;
        }

        void WriteHeader(DataWriter writer, long[] sizes)
        {
            const uint HeaderSize = 0x5C;

            // Calculate the "logical" offset.
            // This does not reflect the offset in the actual file,
            // but how it would be if the layers were in order.
            long level1LogicalOffset = 0x00; // first level
            long level2LogicalOffset = level1LogicalOffset + sizes[1].Pad(BlockSize);
            long level3LogicalOffset = level2LogicalOffset + sizes[2].Pad(BlockSize);

            writer.Write(MagicId, nullTerminator: false);
            writer.Write(Version);
            writer.Write((int)sizes[0]);

            writer.Write(level1LogicalOffset);
            writer.Write(sizes[1]);
            writer.Write(BlockSizeLog);
            writer.Write(0x00); // reserved

            writer.Write(level2LogicalOffset);
            writer.Write(sizes[2]);
            writer.Write(BlockSizeLog);
            writer.Write(0x00); // reserved

            writer.Write(level3LogicalOffset);
            writer.Write(sizes[3]);
            writer.Write(BlockSizeLog);
            writer.Write(0x00); // reserved

            writer.Write(HeaderSize);
        }
    }
}
