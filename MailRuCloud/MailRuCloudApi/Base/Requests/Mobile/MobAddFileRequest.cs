using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using System.Text;
using YaR.MailRuCloud.Api.Base.Requests.Web;

namespace YaR.MailRuCloud.Api.Base.Requests.Mobile
{
    class MobAddFileRequest : BaseRequest<string>
    {
        private readonly string _token;
        private readonly string _fullPath;
        private readonly byte[] _hash;
        private readonly long _size;

        public MobAddFileRequest(CloudApi cloudApi, string token, string fullPath, byte[] hash, long size) : base(cloudApi)
        {
            _token = token;
            _fullPath = fullPath;
            _hash = hash;
            _size = size;
        }

        protected override string RelationalUri => $"https://cloclo2.datacloudmail.ru/meta/?token={_token}&client_id=cloud-android";

        protected override byte[] CreateHttpContent()
        {
            var stream = new RequestBodyStream();
            stream.WritePu16(AddFileOperation);
            stream.WritePu16(Revision);
            stream.WriteString(_fullPath);
            stream.WritePu64(_size);

            //DateTimeOffset dto = new DateTimeOffset(2012, 6, 29, 21, 11, 0, TimeSpan.Zero);
            //var unixtime = dto.ToUnixTimeSeconds();
            DateTimeOffset dto = new DateTime(2012, 6, 29, 21, 11, 0).ToUniversalTime();
            var unixtime = (long)(dto - new DateTime(1970, 1, 1)).TotalSeconds;
            stream.WritePu64(unixtime);
            stream.WritePu32(00);

            stream.Write(_hash);
            stream.WritePu32(UnknownFinal);

            var body = stream.GetBytes();
            return body;
        }

        private const byte AddFileOperation = 103;
        private const int Revision = 0;
        private const byte UnknownFinal = 03;
    }

    class RequestBodyStream 
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

        public byte[] GetBytes()
        {
            _stream.Seek(0, SeekOrigin.Begin);
            return _stream.ToArray();
        }
    }
}
