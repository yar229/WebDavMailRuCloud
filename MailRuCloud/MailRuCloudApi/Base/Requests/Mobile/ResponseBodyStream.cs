using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace YaR.MailRuCloud.Api.Base.Requests.Mobile
{
    class ResponseBodyStream
    {
        private readonly BinaryReader _stream;

        public ResponseBodyStream(Stream stream)
        {
            _stream = new BinaryReader(stream);
            OperationResult = (OperationResult)ReadShort();
        }
        public short ReadShort()
        {
            return (short)(ReadInt() & 255);
        }

        public int ReadInt()
        {
            int b = _stream.ReadInt16();
            if (b == -1)
                throw new Exception("End of stream");
            return b;
        }

        public OperationResult OperationResult { get; }

        public int ReadIntSpl()
        {
            return (ReadInt() & 255) | ((ReadInt() & 255) << 8);
        }

        public byte[] ReadNBytes(int count)
        {
            byte[] bArr = new byte[count];
            for (int i = 0; i < count; i++)
            {
                bArr[i] = (byte)ReadInt();
            }
            return bArr;
        }

        public ulong ReadBigNumber()
        {
            int i = 0;
            byte[] buffer = new byte[8];
            int b;
            do
            {
                b = ReadInt();
                int lo = b & 127;
                int rem = 7 - (i / 8);
                int div = i % 8;
                buffer[rem] = (byte) (buffer[rem] | ((lo << div) & 255));
                lo >>= 8 - div;
                if (lo == 0 || rem != 0)
                {
                    if (lo != 0 && div > 0)
                    {
                        rem--;
                        buffer[rem] = (byte)(lo | buffer[rem]);
                    }
                    i = (byte) (i + 7);
                }
                else
                    throw new Exception("Pu64 error");
            } while ((b & 128) != 0);
            return Convert.ToUInt64(buffer);
        }
    }
}
