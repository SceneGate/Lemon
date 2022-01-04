// Copyright (c) 2020 SceneGate

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
namespace SceneGate.Lemon.Containers.Converters
{
    using System;
    using System.Security.Cryptography;
    using SceneGate.Lemon.Titles;
    using Yarhl.FileFormat;
    using Yarhl.FileSystem;
    using Yarhl.IO;

    /// <summary>
    /// Convert a NodeContainer with CIA sections into binary format.
    /// </summary>
    /// <remarks>
    /// <p>The initialization is optional. If no stream is provided, a new one
    /// will be created on memory. This may consume large RAM memory for big games.</p>
    /// <p>This converter expects to have a node with the following binary
    /// children: certs_chain, ticket, title (TMD), content/program,
    /// content/manual (optional), content/download_play (optional).</p>
    /// <p>This converter expects to receive consistent nodes. This means it expected
    /// that the TMD already has the updated chunks of the content and that the
    /// metadata match the exHeader and ExeFS info.</p>
    /// </remarks>
    public class NodeContainer2BinaryCia :
        IConverter<NodeContainerFormat, BinaryFormat>,
        IInitializer<DataStream>
    {
        const int BlockSize = 64;
        const int ContentIndexSize = 0x2000;
        const int HeaderSize = 0x20 + ContentIndexSize;
        const ushort CiaType = 0; // the type of format, always 0 for 3DS
        const ushort CiaVersion = 0; // the version of the format

        DataStream outputStream;

        /// <summary>
        /// Initializes the converter with the output stream.
        /// </summary>
        /// <param name="parameters">The stream to write the new CIA.</param>
        public void Initialize(DataStream parameters)
        {
            outputStream = parameters;
        }

        /// <summary>
        /// Converts a container into a binary CIA.
        /// </summary>
        /// <param name="source">The container with the CIA files.</param>
        /// <returns>The new binary format with the CIA.</returns>
        public BinaryFormat Convert(NodeContainerFormat source)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            var binary = (outputStream != null) ? new BinaryFormat(outputStream) : new BinaryFormat();
            binary.Stream.Position = 0;

            var writer = new DataWriter(binary.Stream) {
                Endianness = EndiannessMode.LittleEndian,
            };

            Node root = source.Root;
            WriteHeader(writer, root);

            WriteFile(writer, root.Children["certs_chain"]);
            WriteFile(writer, root.Children["ticket"]);
            WriteTitle(writer, root.Children["content"], root.Children["title"]);
            WriteContent(writer, root.Children["content"], root.Children["title"]);
            WriteFile(writer, root.Children["metadata"], false);

            return binary;
        }

        void WriteHeader(DataWriter writer, Node root)
        {
            writer.Write(HeaderSize);
            writer.Write(CiaType);
            writer.Write(CiaVersion);
            writer.Write((uint)(root.Children["certs_chain"]?.Stream?.Length ?? 0));
            writer.Write((uint)(root.Children["ticket"]?.Stream?.Length ?? 0));
            writer.Write((uint)(root.Children["title"]?.Stream?.Length ?? 0));
            writer.Write((uint)(root.Children["metadata"]?.Stream?.Length ?? 0));
            writer.Write(0L); // placeholder for content size
            writer.WriteTimes(0, ContentIndexSize); // placeholder for content index
            writer.WritePadding(0x00, BlockSize);
        }

        void WriteFile(DataWriter writer, Node file, bool isRequired = true)
        {
            if (file == null && isRequired) {
                throw new System.IO.FileNotFoundException("Missing CIA file");
            }

            if (file.Format is not IBinary) {
                throw new FormatException("Cannot write file as it is not binary");
            }

            file.Stream.WriteTo(writer.Stream);
            writer.WritePadding(0x00, BlockSize);
        }

        void WriteTitle(DataWriter writer, Node content, Node titleNode)
        {
            if (titleNode == null)
                throw new FormatException("Missing game title");

            var title = (TitleMetadata)ConvertFormat.With<Binary2TitleMetadata>(titleNode.Format);

            var reader = new DataReader(titleNode.Stream) {
                Endianness = EndiannessMode.BigEndian,
            };

            writer.Endianness = EndiannessMode.BigEndian;

            writer.Write(title.SignType);
            reader.Stream.Position = 4;
            writer.Write(reader.ReadBytes(title.SignSize));
            writer.Write(title.SignatureIssuer.PadRight(0x40, '\0'));
            writer.Write(title.Version);
            writer.Write(title.CaCrlVersion);
            writer.Write(title.SignerCrlVersion);
            writer.Write((byte)0x0);

            writer.Write(title.SystemVersion);
            writer.Write(title.TitleId);
            writer.Write(title.TitleType);
            writer.Write(title.GroupId);

            writer.Write(title.SaveSize);
            writer.Write(title.SrlPrivateSaveSize);
            writer.Write(new byte[0x4]);

            writer.Write(title.SrlFlag);
            writer.Write(new byte[0x31]);

            writer.Write(title.AccessRights);
            writer.Write(title.TitleVersion);
            writer.Write((short)title.Chunks.Count);
            writer.Write((short)title.BootContent);
            writer.Write(new byte[0x2]);

            long hashContentInfoPosition = writer.Stream.Position;

            // We'll write the content info records and its hash later
            writer.Write(new byte[0x20]);
            writer.Write(new byte[0x900]);

            // Start writing the content chunk records into a stream so we can later hash it
            int contentChunksSize = 0x30 * (short)title.Chunks.Count;
            var contentChunksBytes = DataStreamFactory.FromArray(new byte[contentChunksSize], 0, contentChunksSize);

            for (int i = 0; i < title.Chunks.Count; i++) {
                string childName = title.Chunks[i].GetChunkName();
                var child = content.Children[childName];
                if (child is null)
                    throw new FormatException($"Missing child: {childName}");
                if (child.Format is not IBinary)
                    throw new FormatException($"Cannot write child {childName} as it is not binary");

                var chunksWriter = new DataWriter(contentChunksBytes) {
                    Endianness = EndiannessMode.BigEndian,
                };

                var currentChunkReader = new DataReader(contentChunksBytes) {
                    Endianness = EndiannessMode.BigEndian,
                };

                chunksWriter.Write(title.Chunks[i].Id);
                chunksWriter.Write(title.Chunks[i].Index);
                int attribute = (int)Enum.Parse(typeof(ContentAttributes), title.Chunks[i].Attributes.ToString());
                chunksWriter.Write((short)attribute);
                chunksWriter.Write(child.Stream.Length);
                WriteSHA256(chunksWriter, child);

                contentChunksBytes.Position -= 0x30;
                writer.Write(currentChunkReader.ReadBytes(0x30));
            }

            long endOfChunksPosition = writer.Stream.Position;

            var chunkReader = new DataReader(contentChunksBytes) {
                Endianness = EndiannessMode.BigEndian,
            };

            chunkReader.Stream.Position = 0;

            writer.Stream.Position = hashContentInfoPosition + 0x20;

            // We'll start reading the original TMD data to get the content info records data
            reader.Stream.Position += 0xC4;

            // As with the content chunk records, we'll be write this first as a separate binary to later calculate the hash on
            var contentInfoRecords = new BinaryFormat();
            var contentInfoRecordsWriter = new DataWriter(contentInfoRecords.Stream) {
                Endianness = EndiannessMode.BigEndian,
            };

            int chunksHashed = 0;
            for (int infoRecord = 0; infoRecord < 64; infoRecord++) {
                contentInfoRecordsWriter.Write(reader.ReadInt16());

                // This short will say how much chunk records needs to be hashed (if they haven't already)
                short contentCommandCount = reader.ReadInt16();
                contentInfoRecordsWriter.Write(contentCommandCount);

                if (contentCommandCount > 0) {
                    var chunksToHash = DataStreamFactory.FromArray(new byte[0x30 * (contentCommandCount + chunksHashed)], 0, 0x30 * (contentCommandCount + chunksHashed));
                    for (int i = 0; i < contentCommandCount; i++) {
                        var chunkWriter = new DataWriter(chunksToHash) {
                            Endianness = EndiannessMode.BigEndian,
                        };

                        chunkWriter.Write(chunkReader.ReadBytes(0x30));
                    }

                    WriteSHA256Stream(contentInfoRecordsWriter, chunksToHash);
                }
                else {
                    contentInfoRecordsWriter.Write(new byte[0x20]);
                }

                chunksHashed += contentCommandCount;
                reader.Stream.Position += 0x20;
            }

            var contentInfoRecordsReader = new DataReader(contentInfoRecords.Stream) {
                Endianness = EndiannessMode.BigEndian,
            };

            contentInfoRecordsReader.Stream.Position = 0;

            writer.Write(contentInfoRecordsReader.ReadBytes((int)contentInfoRecords.Stream.Length));

            writer.Stream.Position = hashContentInfoPosition;
            WriteSHA256Stream(writer, contentInfoRecords.Stream);

            writer.Stream.Position = endOfChunksPosition;

            writer.Write(new byte[0x1c]);
            writer.Endianness = EndiannessMode.LittleEndian;
        }

        void WriteSHA256(DataWriter writer, Node file)
        {
            using (SHA256 sha256 = SHA256.Create()) {
                try {
                    if (file == null) {
                        writer.Write(new byte[0x20]);
                        return;
                    }

                    file.Stream.Position = 0;

                    writer.Write(sha256.ComputeHash(file.Stream));
                } catch (Exception ex) {
                    throw new FormatException(ex.Message);
                }
            }
        }

        void WriteSHA256Stream(DataWriter writer, DataStream stream)
        {
            using (SHA256 sha256 = SHA256.Create()) {
                try {
                    stream.Position = 0;

                    writer.Write(sha256.ComputeHash(stream));
                } catch (Exception ex) {
                    throw new FormatException(ex.Message);
                }
            }
        }

        void WriteContent(DataWriter writer, Node content, Node titleNode)
        {
            if (titleNode == null)
                throw new FormatException("Missing game title");

            var title = (TitleMetadata)ConvertFormat.With<Binary2TitleMetadata>(titleNode.Format);

            int contentBitset = 0;
            long contentSize = 0;
            for (int i = 0; i < title.Chunks.Count; i++) {
                string childName = title.Chunks[i].GetChunkName();
                var child = content.Children[childName];
                if (child is null)
                    throw new FormatException($"Missing child: {childName}");
                if (child.Format is not IBinary)
                    throw new FormatException($"Cannot write child {childName} as it is not binary");

                child.Stream.WriteTo(writer.Stream);
                contentSize += child.Stream.Length;
                contentBitset |= 1 << (7 - i); // one high bit per content
            }

            // Update header values
            ((DataStream)writer.Stream).PushToPosition(0x18);
            writer.Write(contentSize);
            writer.Write(contentBitset);
            ((DataStream)writer.Stream).PopPosition();
        }
    }
}
