using System;
using System.IO;

namespace YaR.Clouds.XTSSharp
{
    class XTSReadOnlyStream : XtsSectorStream
    {
        private readonly Stream _baseStream;
        private readonly XtsCryptoTransform _decryptor;

        public XTSReadOnlyStream(Stream baseStream, Xts xts, int sectorSize, ulong startSector, int trimStart, uint trimEnd) : base(baseStream, xts, sectorSize)
        {
            _baseStream = baseStream;
            _tempBuffer = new byte[SectorSize];
            _decryptor = xts.CreateDecryptor();
            _currentSector = startSector;
            Length = baseStream.Length - trimStart - trimEnd;

            if (trimStart > 0)
            {
                int read = ReadExactly(_baseStream, _tempBuffer, sectorSize);
                _decryptor.TransformBlock(_tempBuffer, 0, SectorSize, _tempBuffer, 0, _currentSector);
                _tempBufferCount = read;
                _tempBufferPos = trimStart;
                _currentSector++;
            }
        }

        private long _position;


        private readonly byte[] _tempBuffer;
        private int _tempBufferPos;
        private int _tempBufferCount;

        private ulong _currentSector;


        public override int Read(byte[] buffer, int offset, int count)
        {
            int totalread = 0;

            if (_tempBufferCount > 0)
            {
                int tbtocopy = Math.Min(count - offset, _tempBufferCount - _tempBufferPos);
                if (tbtocopy > 0)
                {
                    Array.Copy(_tempBuffer, _tempBufferPos, buffer, offset, tbtocopy);
                    offset += tbtocopy;
                    _tempBufferPos += tbtocopy;
                    totalread += tbtocopy;
                    _position += tbtocopy;
                    if (_tempBufferPos == _tempBufferCount)
                    {
                        _tempBufferPos = 0;
                        _tempBufferCount = 0;
                    }
                }
            }

            while (offset < count && _position < Length)
            {
                int read = ReadExactly(_baseStream, _tempBuffer, SectorSize);
                _decryptor.TransformBlock(_tempBuffer, 0, SectorSize, _tempBuffer, 0, _currentSector);

                _currentSector++;
                _tempBufferCount = read;
                _tempBufferPos = 0;

                int tocopy = Math.Min(count - offset, read);

                Array.Copy(_tempBuffer, _tempBufferPos, buffer, offset, tocopy);
                _tempBufferPos += tocopy;
                offset += tocopy;
                totalread += tocopy;
                _position += tocopy;

                if (_tempBufferPos == _tempBufferCount)
                {
                    _tempBufferPos = 0;
                    _tempBufferCount = 0;
                }
            }

            return totalread;
        }


        private static int ReadExactly(Stream stream, byte[] buffer, int count)
        {
            int offset = 0;
            int totalread = 0;
            while (offset < count)
            {
                int read = stream.Read(buffer, offset, count - offset);
                totalread += read;
                if (read == 0)
                    return totalread;
                offset += read;
            }
            System.Diagnostics.Debug.Assert(offset == count);
            return totalread;
        }

        public override long Length { get; }

        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotSupportedException();
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotSupportedException();
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (!disposing) return;

            _decryptor.Dispose();
        }
    }
}