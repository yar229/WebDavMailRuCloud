using System;
using System.Net;
using YaR.MailRuCloud.Api.Base.Auth;
using YaR.MailRuCloud.Api.Base.Requests.WebBin.Types;

namespace YaR.MailRuCloud.Api.Base.Requests.WebBin
{
    class CreateFolderRequest : BaseRequestMobile<CreateFolderRequest.Result>
    {
        private readonly string _fullPath;

        public CreateFolderRequest(HttpCommonSettings settings, IAuth auth, string metaServer, string fullPath)
            : base(settings, auth, metaServer)
        {
            _fullPath = fullPath;
        }

        protected override byte[] CreateHttpContent()
        {
            using (var stream = new RequestBodyStream())
            {
                stream.WritePu16((byte)Operation.CreateFolder);
                stream.WritePu16(Revision);
                stream.WriteString(_fullPath);
                stream.WritePu32(0);
                var body = stream.GetBytes();
                return body;
            }
        }

        protected override RequestResponse<Result> DeserializeMessage(ResponseBodyStream data)
        {
            if (data.OperationResult == OperationResult.Ok)
            {
                return new RequestResponse<Result>
                {
                    Ok = true,
                    Result = new Result
                    {
                        OperationResult = data.OperationResult,
                        Path = _fullPath
                    }
                };
            }

            throw new Exception($"{nameof(CreateFolderRequest)} failed with result code {data.OperationResult}");
        }

        private const int Revision = 0;

        public class Result : BaseResponseResult
        {
            public string Path { get; set; }
        }
    }
}