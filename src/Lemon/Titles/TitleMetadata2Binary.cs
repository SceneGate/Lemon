// TitleMetadata2Binary.cs
//
// Author:
//       Maxwell Ruiz maxwellaquaruiz@gmail.com
//
// Copyright (c) 2022 Maxwell Ruiz
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
namespace SceneGate.Lemon.Titles
{
    using System.Collections.ObjectModel;
    using System.Security.Cryptography;
    using Yarhl.FileFormat;
    using Yarhl.IO;

    /// <summary>
    /// Convert a TitleMetadata object into binary format.
    /// </summary>
    /// <remarks>
    /// It updates the record's hashes and TMD hash. It does not update the
    /// chunks (CIA content children) hashes or lengths.
    /// </remarks>
    public class TitleMetadata2Binary : IConverter<TitleMetadata, BinaryFormat>
    {
        private const int NumContentInfo = 64;
        private static readonly byte[] EmptyRecord = new byte[0x24];

        /// <summary>
        /// Converts a title metadata object into a binary format.
        /// </summary>
        /// <param name="source">The object to serialize.</param>
        /// <returns>The serialized title metadata.</returns>
        public BinaryFormat Convert(TitleMetadata source)
        {
            var binary = new BinaryFormat();
            var writer = new DataWriter(binary.Stream) {
                Endianness = EndiannessMode.BigEndian,
            };

            WriteHeader(source, writer);

            // hash placeholder
            writer.Stream.PushCurrentPosition();
            writer.WriteTimes(0x00, 0x20);

            // An info record is associated with one or more "commands" or chunks.
            // To update the hash in the record, we need to write its chunks and hash them.
            // We write chunks in a separate stream as we hash them.
            using var chunksStream = new DataStream();
            int chunksWritten = 0;

            long recordsInfoStart = writer.Stream.Position;
            long recordsLength = NumContentInfo * 0x24;
            for (int i = 0; i < NumContentInfo; i++) {
                if (i >= source.InfoRecords.Count || source.InfoRecords[i].IsEmpty) {
                    writer.Write(EmptyRecord);
                    continue;
                }

                WriteRecord(source.InfoRecords[i], source.Chunks, writer, chunksWritten, chunksStream);
                chunksWritten += source.InfoRecords[i].CommandCount;
            }

            // Write chunks
            chunksStream.WriteTo(writer.Stream);

            // Now we have the records, hash them and write it
            source.Hash = SHA256FromStream(writer.Stream, recordsInfoStart, recordsLength);

            writer.Stream.PopPosition();
            writer.Write(source.Hash);

            return binary;
        }

        private static void WriteHeader(TitleMetadata source, DataWriter writer)
        {
            writer.Write(source.SignType);
            writer.Write(source.Signature);
            writer.Write(source.SignatureIssuer, 0x40);
            writer.Write(source.Version);
            writer.Write(source.CaCrlVersion);
            writer.Write(source.SignerCrlVersion);
            writer.Write((byte)0x0); // Reserved

            writer.Write(source.SystemVersion);
            writer.Write(source.TitleId);
            writer.Write(source.TitleType);
            writer.Write(source.GroupId);
            writer.Write(source.SaveSize);
            writer.Write(source.SrlPrivateSaveSize);
            writer.Write(0); // Reserved

            writer.Write(source.SrlFlag);
            writer.Write(new byte[0x31]); // Reserved

            writer.Write(source.AccessRights);
            writer.Write(source.TitleVersion);
            writer.Write((short)source.Chunks.Count);
            writer.Write((short)source.BootContent);
            writer.Write((short)0); // Padding
        }

        private static void WriteRecord(
            ContentInfoRecord record,
            Collection<ContentChunkRecord> chunks,
            DataWriter writer,
            int firstChunkIndex,
            DataStream chunksStream)
        {
            long chunksStart = chunksStream.Position;
            long chunksLength = 0x30 * record.CommandCount;
            var chunksWriter = new DataWriter(chunksStream) {
                Endianness = EndiannessMode.BigEndian,
            };

            for (int k = 0; k < record.CommandCount; k++) {
                ContentChunkRecord chunk = chunks[firstChunkIndex + k];
                WriteChunkInfo(chunk, chunksWriter);
            }

            record.Hash = SHA256FromStream(chunksStream, chunksStart, chunksLength);

            writer.Write(record.IndexOffset);
            writer.Write(record.CommandCount);
            writer.Write(record.Hash);
        }

        private static void WriteChunkInfo(ContentChunkRecord chunkInfo, DataWriter writer)
        {
            writer.Write(chunkInfo.Id);
            writer.Write(chunkInfo.Index);
            writer.Write((short)chunkInfo.Attributes);
            writer.Write(chunkInfo.Size);
            writer.Write(chunkInfo.Hash);
        }

        private static byte[] SHA256FromStream(Stream stream, long offset, long length)
        {
            using var sha256 = SHA256.Create();
            try {
                using var substream = new DataStream(stream, offset, length);
                return sha256.ComputeHash(substream);
            } catch (Exception ex) {
                throw new FormatException($"Failed to perform SHA256 during TMD update", ex);
            }
        }
    }
}
