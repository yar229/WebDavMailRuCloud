using System;
using System.Linq;
using System.Net;
using YaR.MailRuCloud.Api.Base.Auth;
using YaR.MailRuCloud.Api.Base.Requests.WebBin.Types;
using YaR.MailRuCloud.Api.Extensions;

namespace YaR.MailRuCloud.Api.Base.Requests.WebBin
{
    class MobAddFileRequest : BaseRequestMobile<MobAddFileRequest.Result>
    {
        private readonly string _fullPath;
        private readonly byte[] _hash;
        private readonly long _size;
        private readonly ConflictResolver _conflictResolver;
        private readonly DateTime _dateTime;

        public MobAddFileRequest(HttpCommonSettings settings, IAuth auth, string metaServer, string fullPath, byte[] hash, long size, DateTime? dateTime, ConflictResolver? conflict) 
            : base(settings, auth, metaServer)
        {
            _fullPath = fullPath;
            _hash = hash ?? new byte[20]; // zero length file
            _size = size;
            _conflictResolver = conflict ?? ConflictResolver.Rewrite;
            _dateTime = (dateTime ?? DateTime.Now).ToUniversalTime();
        }

        public MobAddFileRequest(HttpCommonSettings settings, IAuth auth, string metaServer, string fullPath, string hash, long size, DateTime? dateTime, ConflictResolver? conflict) 
            : this(settings, auth, metaServer, fullPath, hash?.HexStringToByteArray(), size, dateTime, conflict)
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

                long mask = ConflictResolver.Rename == _conflictResolver  // 1 = overwrite, 55 = don't add if not changed, add with rename if changed
                    ? 55
                    : 1;
                stream.WritePu32(mask);

                if ((mask & 32) != 0)
                {
                    stream.Write(_hash);
                    stream.WritePu64(_size);
                }

                var body = stream.GetBytes();
                return body;
            }
        }

        private static readonly OpResult[] SuccessCodes = { OpResult.Ok, OpResult.NotModified, OpResult.Dunno04, OpResult.Dunno09};

        protected override RequestResponse<Result> DeserializeMessage(ResponseBodyStream data)
        {
            var opres = (OpResult)(int)data.OperationResult;

            if (!SuccessCodes.Contains(opres))
                throw new Exception($"{nameof(MobAddFileRequest)} failed with operation result code {opres} ({(int)opres})");

            var res = new RequestResponse<Result>
            {
                Ok = true,
                Result = new Result
                {
                    IsSuccess = true,
                    OperationResult = data.OperationResult,
                    Path = _fullPath
                }
            };

            return res;
        }

        private const int Revision = 0;

        private enum OpResult
        {
            Ok = 0,
            Error01 = 1,
            Dunno04 = 4,
            WrongPath = 5,
            NoFreeSpace = 7,
            Dunno09 = 9,
            NotModified = 12,
            FailedA = 253,
            FailedB = 254
        }

        public class Result : BaseResponseResult
        {
            public bool IsSuccess { get; set; }
            public string Path { get; set; }
        }
    }
}
