using System;
using System.Linq;
using YaR.MailRuCloud.Api.Extensions;

namespace YaR.MailRuCloud.Api.Base.Requests.Mobile
{
    class MobAddFileRequest : BaseRequestMobile<MobAddFileRequest.Result>
    {
        private readonly string _fullPath;
        private readonly byte[] _hash;
        private readonly long _size;
        private readonly DateTime _dateTime;

        public MobAddFileRequest(CloudApi cloudApi, string token, string metaServer, string fullPath, byte[] hash, long size, DateTime? dateTime) 
            : base(cloudApi, token, metaServer)
        {
            _fullPath = fullPath;
            _hash = hash;
            _size = size;
            _dateTime = (dateTime ?? DateTime.Now).ToUniversalTime();
        }

        public MobAddFileRequest(CloudApi cloudApi, string token, string metaServer, string fullPath, string hash, long size, DateTime? dateTime) 
            : this(cloudApi, token, metaServer, fullPath, hash.HexStringToByteArray(), size, dateTime)
        {
        }

        protected override byte[] CreateHttpContent()
        {
            using (var stream = new RequestBodyStream())
            {
                stream.WritePu16((byte)Operation.AddFile);
                stream.WritePu16(Revision);
                stream.WriteString(_fullPath);
                stream.WritePu64(_size);

                stream.WritePu64(_dateTime.ToUnix());
                stream.WritePu32(00);

                stream.Write(_hash);
                stream.WritePu32(UnknownFinal);

                var body = stream.GetBytes();
                return body;
            }
        }

        private static readonly OperationResult[] SuccessCodes = {OperationResult.Ok, OperationResult.AlreadyExists04, OperationResult.AlreadyExists09};

        protected override RequestResponse<Result> DeserializeMessage(ResponseBodyStream data)
        {
            if (!SuccessCodes.Contains(data.OperationResult))
                throw new Exception($"{nameof(MobAddFileRequest)} failed with operation result code {data.OperationResult}");

            var res = new RequestResponse<Result>
            {
                Ok = data.OperationResult == OperationResult.Ok,
                Result = new Result
                {
                    OperationResult = data.OperationResult,
                    Path = _fullPath
                }
            };

            return res;
        }

        private const int Revision = 0;
        private const byte UnknownFinal = 03;

        public class Result : BaseResponseResult
        {
            public string Path { get; set; }
        }
    }
}
