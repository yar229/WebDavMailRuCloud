using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using YaR.MailRuCloud.Api.Extensions;

namespace YaR.MailRuCloud.Api.Base.Requests.Mobile
{
    class ResponseBodyStream : IDisposable
    {
        private readonly Stream _stream;

        public ResponseBodyStream(Stream stream)
        {
            _stream = stream;
            OperationResult = (OperationResult)ReadShort();
        }
        public short ReadShort()
        {
            return (short)(ReadInt() & 255);
        }

        public int ReadInt()
        {
            int b = _stream.ReadByte();

            Debug.Write($"{b:X} ");

            if (b == -1)
                throw new Exception("End of stream");
            return b;
        }

        public OperationResult OperationResult { get; }

        public int ReadIntSpl()
        {
            Debug.Write($"{nameof(ReadIntSpl)}() = ");

            int res = (ReadInt() & 255) | ((ReadInt() & 255) << 8);

            Debug.WriteLine($" = {res}");
            return res;
        }

        public byte[] ReadNBytes(int count)
        {
            Debug.Write($"{nameof(ReadNBytes)}({count}) = ");
            byte[] bytes = new byte[count];
            for (int i = 0; i < count; i++)
            {
                bytes[i] = (byte)ReadInt();
            }
            Debug.WriteLine("");

            return bytes;
        }

        public ulong ReadULong()
        {
            Debug.Write($"{nameof(ReadULong)}() = ");
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

            UInt64 res = BitConverter.ToUInt64(buffer, 0);
            Debug.WriteLine($" = {res}");
            return res;
        }

        public long ReadPu32()
        {
            Debug.Write($"(");
            long j = 0;
            int shift = 0;
            int b;
            do
            {
                b = ReadInt();
                j |= (long) (b & 127) << shift;
                shift = (byte) (shift + 7);
            } while ((b & 128) != 0);

            Debug.Write($" = {j}) ");
            return j;
        }


        public byte[] ReadBytesByLength()
        {
            var len = ReadPu32();
            return ReadNBytes((int)len);
        }

        public string ReadNBytesAsString(int bytesCount)
        {
            var data = ReadNBytes(bytesCount);
            string res = Encoding.UTF8.GetString(data);
            Debug.WriteLine($"string = {res}");
            return res;
        }



        public byte[] ReadAllBytes()
        {
            const int bufferSize = 4096;
            using (var ms = new MemoryStream())
            {
                byte[] buffer = new byte[bufferSize];
                int count;
                while ((count = _stream.Read(buffer, 0, buffer.Length)) != 0)
                    ms.Write(buffer, 0, count);
                return ms.ToArray();
            }

        }

        public TreeId ReadTreeId()
        {
            return TreeId.FromStream(this);
        }

        public DateTime ReadDate()
        {
            var val = ReadULong();
            return (val / 1000).ToDateTime();
        }

        public void Dispose()
        {
            //_stream?.Dispose();
        }
    }
}
