using System;
using System.IO;
using System.Security.Cryptography;
using YaR.WebDavMailRu.CloudStore.XTSSharp;

namespace YaR.MailRuCloud.Api.XTSSharp
{
    class XTSReadOnlyStream : Stream
    {
        public const int DefaultSectorSize = 512;

        private readonly XtsCryptoTransform _decryptor;
        private readonly Stream _baseStream;

        private ulong _currentSector;
        private readonly int _sectorSize;

        /// <summary>
        /// Creates a new stream
        /// </summary>
        /// <param name="baseStream">The base stream</param>
        /// <param name="xts">The xts transform</param>
        /// <param name="sectorSize">Sector size</param>
        public XTSReadOnlyStream(Stream baseStream, Xts xts, int sectorSize = DefaultSectorSize)
        {
            _baseStream = baseStream;

            _decryptor = xts.CreateDecryptor();

            _sectorSize = sectorSize;
        }

        private byte[] _readBuffer = new byte[256 * 1024];
        private int _readBufferCount;
        private int _readBufferPos;

        public override int Read(byte[] buffer, int offset, int count)
        {
            if (count == 0)
                return 0;
            int countless = count - _readBufferCount;
            int bytesToRead = countless % _sectorSize > 0 ? countless / _sectorSize + _sectorSize : countless;
            if (_readBuffer.Length < _readBufferCount + bytesToRead)
                Array.Resize(ref _readBuffer, _readBufferCount + bytesToRead);

            int read = _baseStream.Read(_readBuffer, _readBufferPos, countless);
            _readBufferCount += read;
            _readBufferPos = 0;

            int countleft = read;
            int totalDecrypted = 0;
            while (_readBufferPos < countleft)
            {
                int bytesToDecrypt = Math.Min(_sectorSize, countleft - _readBufferPos);
                _decryptor.TransformBlock(_readBuffer, _readBufferPos, bytesToDecrypt, buffer, offset, _currentSector);
                offset += bytesToDecrypt;
                countleft -= bytesToDecrypt;
                _readBufferPos += bytesToDecrypt;
                totalDecrypted += bytesToDecrypt;
                _currentSector++;
            }

            if (_readBufferPos < _readBufferCount)
            {
                Array.Copy(_readBuffer, _readBufferPos, _readBuffer, 0, _readBufferCount - _readBufferPos);
                _readBufferCount -= _readBufferPos;
                _readBufferPos = 0;
                _currentSector--;
            }

            return totalDecrypted;
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (!disposing) return;

            _baseStream.Dispose();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotImplementedException();
        }

        public override void Flush()
        {
            throw new NotImplementedException();
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotImplementedException();
        }

        public override void SetLength(long value)
        {
            throw new NotImplementedException();
        }

        public override bool CanRead => true;
        public override bool CanSeek => true; //TODO: really not
        public override bool CanWrite => false;
        public override long Length => _baseStream.Length;
        public override long Position { get; set; }
    }
}