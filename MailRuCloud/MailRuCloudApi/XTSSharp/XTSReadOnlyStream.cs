using System;
using System.IO;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using YaR.WebDavMailRu.CloudStore.XTSSharp;

namespace YaR.MailRuCloud.Api.XTSSharp
{
    class XTSReadOnlyStream : XtsSectorStream
    {
        private int _trimStart;
        private readonly byte[] _tempBuffer;

        public XTSReadOnlyStream(Stream baseStream, Xts xts, int sectorSize, int trimStart, int trimEnd) : base(baseStream, xts, sectorSize)
        {
            _trimStart = trimStart;

            Length = baseStream.Length - _trimStart - trimEnd;

            _tempBuffer = new byte[SectorSize];
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            int totalRead = 0;
            int read = -1;
            int toread;
            while ((toread = Math.Min(SectorSize, count - offset)) > 0 && read != 0)
            {
                read = base.Read(_tempBuffer, 0, toread);
                Array.Copy(_tempBuffer, _trimStart, buffer, offset + _trimStart, read - _trimStart);
                offset += read - _trimStart;
                totalRead += read - _trimStart;

                if (_trimStart > 0) _trimStart = 0;
            }

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