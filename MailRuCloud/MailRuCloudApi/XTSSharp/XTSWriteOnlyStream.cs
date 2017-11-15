using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using YaR.WebDavMailRu.CloudStore.XTSSharp;

namespace YaR.MailRuCloud.Api.XTSSharp
{
    class XTSWriteOnlyStream : Stream
    {
        public const int DefaultSectorSize = 512;

        private readonly XtsCryptoTransform _encryptor;
        private readonly Stream _baseStream;

        private ulong _currentSector;
        private readonly int _sectorSize;
        private readonly byte[] _encriptedBuffer;
        private readonly byte[] _sectorBuffer;

        /// <summary>
        /// Creates a new stream
        /// </summary>
        /// <param name="baseStream">The base stream</param>
        /// <param name="xts">The xts transform</param>
        /// <param name="sectorSize">Sector size</param>
        public XTSWriteOnlyStream(Stream baseStream, Xts xts, int sectorSize = DefaultSectorSize)
        {
            _baseStream = baseStream;

            _encryptor = xts.CreateEncryptor();

            _sectorSize = sectorSize;
            _encriptedBuffer = new byte[sectorSize];
            _sectorBuffer = new byte[sectorSize];
        }


        private int _sectorBufferCount;

        public override void Write(byte[] buffer, int offset, int count)
        {
            if (count == 0)
                return;

            while (count > 0)
            {
                int bytesToCopy = Math.Min(count, _sectorSize - _sectorBufferCount);
                Buffer.BlockCopy(buffer, offset, _sectorBuffer, _sectorBufferCount, bytesToCopy);
                _sectorBufferCount += bytesToCopy;
                offset += bytesToCopy;
                count -= bytesToCopy;

                if (_sectorBufferCount == _sectorSize)
                {
                    //sector filled
                    int transformedCount = _encryptor.TransformBlock(_sectorBuffer, 0, _sectorSize, _encriptedBuffer, 0, _currentSector);
                    //Array.Copy(_sectorBuffer, 0, _encriptedBuffer, 0, _sectorSize);
                    _baseStream.Write(_encriptedBuffer, 0, _sectorSize);

                    _currentSector++;
                    _sectorBufferCount = 0;
                }
            }
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (!disposing) return;

            if (_sectorBufferCount > 0)
            {
                int transformedCount = _encryptor.TransformBlock(_sectorBuffer, 0, _sectorBufferCount, _encriptedBuffer, 0, _currentSector);
                //Array.Copy(_sectorBuffer, 0, _encriptedBuffer, 0, _sectorBufferCount);
                _baseStream.Write(_encriptedBuffer, 0, _sectorBufferCount);
            }
            _baseStream.Dispose();

        }


        public override void Flush()
        {
            throw new NotImplementedException();
        }

        public override int Read(byte[] buffer, int offset, int count)
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

        public override bool CanRead { get; }
        public override bool CanSeek { get; }
        public override bool CanWrite => true;
        public override long Length { get; }
        public override long Position { get; set; }
    }
}
