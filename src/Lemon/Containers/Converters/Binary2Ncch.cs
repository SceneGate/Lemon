// Copyright (c) 2019 SceneGate

// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:

// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.

// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.
namespace Lemon.Containers.Converters
{
    using System;
    using System.Diagnostics.CodeAnalysis;
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

        [SuppressMessage("Reliability", "CA2000", Justification = "Transfer ownership")]
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
