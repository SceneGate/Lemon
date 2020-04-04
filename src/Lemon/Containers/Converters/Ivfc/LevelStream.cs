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
namespace Lemon.Containers.Converters.Ivfc
{
    using System;
    using System.Security.Cryptography;
    using Yarhl.IO;
    using Yarhl.IO.StreamFormat;

    /// <summary>
    /// IVFC level stream.
    /// </summary>
    internal class LevelStream : IStream
    {
        readonly IStream stream;
        readonly bool managedStream;
        SHA256 sha;

        /// <summary>
        /// Initializes a new instance of the <see cref="LevelStream"/> class
        /// and use a stream in-memory.
        /// </summary>
        /// <param name="blockSize">Block size for padding and hash.</param>
        public LevelStream(int blockSize)
        {
            BlockSize = blockSize;
            stream = new RecyclableMemoryStream();
            managedStream = true;

            sha = SHA256.Create();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LevelStream" /> class.
         /// </summary>
        /// <param name="blockSize">Block size for padding and hash.</param>
        /// <param name="stream">The underlying stream.</param>
        public LevelStream(int blockSize, IStream stream)
        {
            BlockSize = blockSize;
            this.stream = stream;
            managedStream = false;

            sha = SHA256.Create();
        }

        /// <summary>
        /// Raises when a new block of data is generated.
        /// </summary>
        public event EventHandler<BlockWrittenEventArgs> BlockWritten;

        /// <summary>
        /// Gets the block size for padding and hash.
        /// </summary>
        public int BlockSize { get; }

        /// <summary>
        /// Gets or sets the position from the start of this stream.
        /// </summary>
        public long Position { get; set; }

        /// <summary>
        /// Gets the length of this stream.
        /// </summary>
        public long Length => stream.Length;

        /// <summary>
        /// Gets a value indicating whether this <see cref="LevelStream" />
        /// has been dispsosed.
        /// </summary>
        public bool Disposed { get; private set; }

        /// <summary>
        /// Sets the length of the stream.
        /// </summary>
        /// <param name="length">The new length of the stream.</param>
        /// <remarks>
        /// Some streams may not implement or support changing the length.
        /// </remarks>
        public void SetLength(long length)
        {
            stream.SetLength(length);
        }

        /// <summary>
        /// Reads the next byte.
        /// </summary>
        /// <returns>The next byte.</returns>
        public byte ReadByte()
        {
            stream.Position = Position;
            Position++;
            return stream.ReadByte();
        }

        /// <summary>
        /// Reads from the stream to the buffer.
        /// </summary>
        /// <returns>The number of bytes read.</returns>
        /// <param name="buffer">Buffer to copy data.</param>
        /// <param name="index">Index to start copying in buffer.</param>
        /// <param name="count">Number of bytes to read.</param>
        public int Read(byte[] buffer, int index, int count)
        {
            stream.Position = Position;
            Position += count;
            return stream.Read(buffer, index, count);
        }

        /// <summary>
        /// Writes a byte.
        /// </summary>
        /// <param name="data">Byte value.</param>
        public void WriteByte(byte data)
        {
            WriteAndUpdateHash(new[] { data }, 0, 1);
        }

        /// <summary>
        /// Writes the a portion of the buffer to the stream.
        /// </summary>
        /// <param name="buffer">Buffer to write.</param>
        /// <param name="index">Index in the buffer.</param>
        /// <param name="count">Bytes to write.</param>
        public void Write(byte[] buffer, int index, int count)
        {
            WriteAndUpdateHash(buffer, index, count);
        }

        /// <summary>
        /// Releases all resource used by the <see cref="LevelStream"/> object.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases all resources used by the object.
        /// </summary>
        /// <param name="freeManaged">Whethever to free the managed resources too.</param>
        protected virtual void Dispose(bool freeManaged)
        {
            if (Disposed) {
                return;
            }

            Disposed = true;
            if (freeManaged) {
                sha.Dispose();
                if (managedStream) {
                    stream.Dispose();
                }
            }
        }

        void WriteAndUpdateHash(byte[] data, int offset, int count)
        {
            while (true) {
                // Repeat until writing the data does not span into the next block.
                long writtenBlocks = Position / BlockSize;
                long futureBlocks = (Position + count) / BlockSize;
                if (futureBlocks == writtenBlocks) {
                    break;
                }

                int posInBlock = (int)(Position % BlockSize);
                int bytesToWrite = BlockSize - posInBlock;

                // It's mandatory to call at least one time to TransformBlock
                // before calling again TransformFinalBlock, so this should
                // make the trick, calling with size 0.
                if (posInBlock == 0 && bytesToWrite > 0)
                {
                    sha.TransformBlock(data, offset, 1, data, offset);
                    WriteStream(data, offset, 1);
                    offset += 1;
                    count -= 1;
                    bytesToWrite -= 1;
                }

                // Send bytes to SHA and to the stream so we update our
                // position too.
                sha.TransformFinalBlock(data, offset, bytesToWrite);
                WriteStream(data, offset, bytesToWrite);

                // Trigger event.
                var eventArgs = new BlockWrittenEventArgs(sha.Hash);
                BlockWritten?.Invoke(this, eventArgs);
                sha.Dispose();
                sha = SHA256.Create();

                // Skip already processed bytes
                offset += bytesToWrite;
                count -= bytesToWrite;
            }

            if (count > 0) {
                sha.TransformBlock(data, offset, count, data, offset);
                WriteStream(data, offset, count);
            }
        }

        void WriteStream(byte[] data, int index, int count)
        {
            stream.Position = Position;
            Position += count;
            stream.Write(data, index, count);
        }
    }
}
