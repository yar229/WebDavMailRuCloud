using System;
using System.IO;
using System.Text;

namespace YaR.MailRuCloud.Api.Base.Requests.Mobile.Types
{
    class RequestBodyStream : IDisposable
    {
        private readonly MemoryStream _stream = new MemoryStream();

        public void WritePu16(int value)
        {
            if (value < 0 || (long)value > 65535)
                throw new Exception("Invalid PU16 " + value);

            WritePu32((long)value);
        }

        public void WritePu32(long value)
        {
            if (value < 0 || value > 4294967295L)
                throw new Exception("Invalid PU32 " + value);

            do
            {
                int i = (int)(127 & value);
                value >>= 7;
                if (value != 0)
                    i |= 128;
                _stream.WriteByte((byte)i);
            } while (value > 0);
        }

        public void WriteString(String value)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(value);
            WritePu32((long)(bytes.Length + 1));
            _stream.Write(bytes, 0, bytes.Length);
            _stream.WriteByte(00);
        }

        public void WritePu64(long value)
        {
            if (value < 0)
                throw new Exception("Invalid PU64 " + value);

            int intValue;
            do {
                int high = ((int)value) & 127;
                value >>= 7;
                intValue = (int)value;
                if (intValue != 0) 
                    high |= 128;
                _stream.WriteByte((byte)high);
            } while (intValue != 0);
        }

        public void Write(byte[] bytes)
        {
            _stream.Write(bytes, 0, bytes.Length);
        }

        public void WriteWithLength(byte[] buffer)
        {
            WritePu32(buffer.Length);
            Write(buffer);
        }


    public byte[] GetBytes()
        {
            _stream.Seek(0, SeekOrigin.Begin);
            return _stream.ToArray();
        }

        public void Dispose()
        {
            _stream?.Dispose();
        }
    }
}