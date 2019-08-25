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
namespace Lemon.Containers.Converters.Ivfc
{
    using System;
    using System.Security.Cryptography;
    using Yarhl.IO;
    using Yarhl.IO.StreamFormat;

    internal class LevelStream : IStream
    {
        readonly IStream stream;
        readonly bool managedStream;
        SHA256 sha;

        public LevelStream(int blockSize)
        {
            BlockSize = blockSize;
            stream = new RecyclableMemoryStream();
            managedStream = true;

            sha = SHA256.Create();
        }

        public LevelStream(int blockSize, IStream stream)
        {
            BlockSize = blockSize;
            this.stream = stream;
            managedStream = false;

            sha = SHA256.Create();
        }

        public event EventHandler<BlockWrittenEventArgs> BlockWritten;

        public int BlockSize { get; }

        public long Position { get; set; }

        public long Length => stream.Length;

        public bool Disposed { get; private set; }

        public byte ReadByte()
        {
            stream.Position = Position;
            Position++;
            return stream.ReadByte();
        }

        public int Read(byte[] buffer, int offset, int count)
        {
            stream.Position = Position;
            Position += count;
            return stream.Read(buffer, offset, count);
        }

        public void WriteByte(byte val)
        {
            WriteAndUpdateHash(new[] { val }, 0, 1);
        }

        public void Write(byte[] buffer, int offset, int count)
        {
            WriteAndUpdateHash(buffer, offset, count);
        }

        public void SetLength(long length)
        {
            stream.SetLength(length);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        void WriteAndUpdateHash(byte[] data, int offset, int count)
        {
            byte[] empty = Array.Empty<byte>();
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

        void Dispose(bool freeManaged)
        {
            if (Disposed) {
                return;
            }

            Disposed = true;
            if (freeManaged && managedStream) {
                stream.Dispose();
            }
        }
    }
}
