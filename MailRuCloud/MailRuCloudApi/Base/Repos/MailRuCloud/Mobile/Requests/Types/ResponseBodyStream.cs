using System;
using System.IO;
using System.Text;
using YaR.Clouds.Extensions;

namespace YaR.Clouds.Base.Repos.MailRuCloud.Mobile.Requests.Types
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

        public OperationResult OperationResult { get; }

        public int ReadIntSpl()
        {
            //Debug.Write($"{nameof(ReadIntSpl)}() = ");

            int res = (ReadInt() & 255) | ((ReadInt() & 255) << 8);

            //Debug.WriteLine($" = {res}");
            return res;
        }

        public byte[] ReadNBytes(long count)
        {
            int total = (int)count;
            byte[] bytes = new byte[total];

            int bytesRead = 0;
            int read;
            while ((read = _stream.Read(bytes, bytesRead, total - bytesRead)) > 0)
                bytesRead += read;

            if (bytesRead < total)
                throw new Exception($"End of stream {nameof(ReadNBytes)}");

            return bytes;
        }

        public int ReadInt()
        {
            int b = _stream.ReadByte();

            //Debug.Write($"{b:X} ");

            if (b == -1)
                throw new Exception("End of stream");
            return b;
        }

        public ulong ReadULong()
        {
            //Debug.Write($"{nameof(ReadULong)}() = ");

            int i = 0;
            byte[] bytes = new byte[8];
            int val;
            do
            {
                val = ReadInt();
                int high = val & 127;
                int rem = 7 - i / 8;
                int div = i % 8;
                bytes[rem] = (byte) (bytes[rem] | ((high << div) & 255));
                high >>= 8 - div;
                if (high == 0 || rem != 0)
                {
                    if (high != 0 && div > 0)
                    {
                        rem--;
                        bytes[rem] = (byte)(high | bytes[rem]);
                    }
                    i = (byte) (i + 7);
                }
                else
                    throw new Exception("Pu64 error");
            } while ((val & 128) != 0);

            Array.Reverse(bytes);
            UInt64 res = BitConverter.ToUInt64(bytes, 0);
            //Debug.WriteLine($" = {res}");
            return res;
        }

        public long ReadPu32()
        {
            //Debug.Write($"(");
            long j = 0;
            int shift = 0;
            int b;
            do
            {
                b = ReadInt();
                j |= (long) (b & 127) << shift;
                shift = (byte) (shift + 7);
            } while ((b & 128) != 0);

            //Debug.Write($" = {j}) ");
            return j;
        }


        public byte[] ReadBytesByLength()
        {
            var len = ReadPu32();
            return ReadNBytes((int)len);
        }

        public string ReadNBytesAsString(long bytesCount)
        {
            var data = ReadNBytes(bytesCount);
            string res = Encoding.UTF8.GetString(data);
            //Debug.WriteLine($"string = {res}");
            return res;
        }

        public string ReadString()
        {
            string str = ReadNBytesAsString(ReadPu32() - 1);

            if (ReadInt() != 0)
                throw new FormatException("String does not ends with zero");

            return str;
        }

        public byte[] ReadAllBytes()
        {
            const int bufferSize = 4096;
            using var ms = new MemoryStream();

            byte[] buffer = new byte[bufferSize];
            int count;
            while ((count = _stream.Read(buffer, 0, buffer.Length)) != 0)
                ms.Write(buffer, 0, count);
            return ms.ToArray();
        }

        public TreeId ReadTreeId()
        {
            return TreeId.FromStream(this);
        }

        public DateTime ReadDate()
        {
            var res = ReadULong().ToDateTime();

            //Debug.WriteLine($"datetime = {res}");

            return res;
        }

        public void Dispose()
        {
            //_stream?.Dispose();
        }
    }
}
