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

        private readonly Xts _xts;
        private XtsCryptoTransform _encryptor;
        private readonly Stream _baseStream;

        private ulong _currentSector = 0;
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

            _xts = xts;
            _encryptor = _xts.CreateEncryptor();

            _sectorSize = sectorSize;
            _encriptedBuffer = new byte[sectorSize];
            _sectorBuffer = new byte[sectorSize];
        }


        private int _sectorBufferCount = 0;

        public override void Write(byte[] buffer, int offset, int count)
        {
            if (count == 0)
                return;

            int bytesToCopy = Math.Min(count, _sectorSize - _sectorBufferCount);
            Buffer.BlockCopy(buffer, offset, _sectorBuffer, _sectorBufferCount, bytesToCopy);
            _sectorBufferCount += bytesToCopy;
            offset += bytesToCopy;
            count -= bytesToCopy;

            if (_sectorBufferCount == _sectorSize)
            {
                //sector filled
                int transformedCount = _encryptor.TransformBlock(buffer, offset, count, _encriptedBuffer, 0, _currentSector);
                base.Write(_encriptedBuffer, 0, transformedCount);

            }



            //encrypt the sector
            int transformedCount = _encryptor.TransformBlock(buffer, offset, count, encriptedBuffer, 0, _currentSector);

            //Console.WriteLine("Encrypting sector {0}", currentSector);

            //write it to the base stream
            base.Write(encriptedBuffer, 0, transformedCount);
        }
    }
}
