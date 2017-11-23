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

        public MobAddFileRequest(CloudApi cloudApi, string token, string fullPath, byte[] hash, long size, DateTime? dateTime) : base(cloudApi)
        {
            _token = token;
            _fullPath = fullPath;
            _hash = hash;
            _size = size;
            _dateTime = (dateTime ?? DateTime.Now).ToUniversalTime();
        }

        protected override string RelationalUri => $"https://cloclo2.datacloudmail.ru/meta/?token={_token}&client_id=cloud-android";

        protected override byte[] CreateHttpContent()
        {
            var stream = new RequestBodyStream();
            stream.WritePu16(AddFileOperation);
            stream.WritePu16(Revision);
            stream.WriteString(_fullPath);
            stream.WritePu64(_size);

            var unixtime = (long)(_dateTime - _epoch).TotalSeconds;
            stream.WritePu64(unixtime);
            stream.WritePu32(00);

            stream.Write(_hash);
            stream.WritePu32(UnknownFinal);

            var body = stream.GetBytes();
            return body;
        }

        private static DateTime _epoch = new DateTime(1970, 1, 1);
        private const byte AddFileOperation = 103;
        private const int Revision = 0;
        private const byte UnknownFinal = 03;
    }
}
