using System;
using System.IO;
using YaR.WebDavMailRu.CloudStore.XTSSharp;

namespace YaR.MailRuCloud.Api.XTSSharp
{
    class XTSReadOnlyStream : XtsSectorStream
    {
        private int _trimStart;
        private readonly byte[] _tempBuffer;

        public XTSReadOnlyStream(Stream baseStream, Xts xts, int sectorSize, int trimStart, uint trimEnd) : base(baseStream, xts, sectorSize)
        {
            _trimStart = trimStart;

            Length = baseStream.Length - _trimStart - trimEnd;

            _tempBuffer = new byte[SectorSize];
        }

        private long _position;

        public override int Read(byte[] buffer, int offset, int count)
        {
            int toread, read, totalRead = 0;
            while (
                offset < count &&
                (toread = Math.Max(Math.Min(SectorSize, count - offset), SectorSize)) > 0 &&
                (read = base.Read(_tempBuffer, 0, toread)) != 0)
            {
                _position += read;
                Array.Copy(_tempBuffer, _trimStart, buffer, offset + _trimStart, read - _trimStart);
                offset += read - _trimStart;
                totalRead += read - _trimStart;

                if (_trimStart > 0) _trimStart = 0;
            }

            if (_position > Length)
                return totalRead - (int)(_position - Length);

            return totalRead;
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
    }
}