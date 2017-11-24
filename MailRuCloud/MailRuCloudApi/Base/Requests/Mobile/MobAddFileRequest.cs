using System;
using System.Collections.Generic;
using System.Numerics;
using YaR.MailRuCloud.Api.Base.Requests.Web;

namespace YaR.MailRuCloud.Api.Base.Requests.Mobile
{
    class MobAddFileRequest : BaseRequest<string>
    {
        private readonly string _token;
        private readonly string _fullPath;
        private readonly byte[] _hash;
        private readonly long _size;
        private readonly DateTime _dateTime;

        public MobAddFileRequest(CloudApi cloudApi, string fullPath, byte[] hash, long size, DateTime? dateTime) 
            : base(cloudApi)
        {
            _token = cloudApi.Account.AuthTokenMobile.Value;
            _fullPath = fullPath;
            _hash = hash;
            _size = size;
            _dateTime = (dateTime ?? DateTime.Now).ToUniversalTime();
        }

        public MobAddFileRequest(CloudApi cloudApi, string fullPath, string hash, long size, DateTime? dateTime) 
            : this(cloudApi, fullPath, StringToByteArray(hash), size, dateTime)
        {
        }

        protected override string RelationalUri
        {
            get
            {
                var meta = CloudApi.Account.MetaServer.Value;
                return $"{meta.Url}?token={_token}&client_id=cloud-android";
            }
        }

        protected override byte[] CreateHttpContent()
        {
            var stream = new RequestBodyStream();
            stream.WritePu16(AddFileOperation);
            stream.WritePu16(Revision);
            stream.WriteString(_fullPath);
            stream.WritePu64(_size);

            var unixtime = ConvertToUnixTimestamp(_dateTime);
            stream.WritePu64(unixtime);
            stream.WritePu32(00);

            stream.Write(_hash);
            stream.WritePu32(UnknownFinal);

            var body = stream.GetBytes();
            return body;
        }

        public static long ConvertToUnixTimestamp(DateTime date)
        {
            DateTime origin = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            TimeSpan diff = date.ToUniversalTime() - origin;

            long seconds = diff.Ticks / TimeSpan.TicksPerSecond;
            return seconds;
        }

        private static byte[] StringToByteArray(String hex)
        {
            int len = hex.Length;
            byte[] bytes = new byte[len / 2];
            for (int i = 0; i < len; i += 2)
                bytes[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);
            return bytes;
        }

        private static DateTime _epoch = new DateTime(1970, 1, 1);
        private const byte AddFileOperation = 103;
        private const int Revision = 0;
        private const byte UnknownFinal = 03;
    }
}
