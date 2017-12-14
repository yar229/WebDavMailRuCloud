using System;
using YaR.MailRuCloud.Api.Base.Requests.Repo;
using YaR.MailRuCloud.Api.Base.Requests.WebBin.Types;

namespace YaR.MailRuCloud.Api.Base.Requests.WebBin
{
    class RenameRequest : BaseRequestMobile<RenameRequest.Result>
    {
        private readonly string _fromPath;
        private readonly string _toPath;

        public RenameRequest(HttpCommonSettings settings, IAuth auth, string metaServer, string fromPath, string toPath)
            : base(settings, auth, metaServer)
        {
            _fromPath = fromPath;
            _toPath = toPath;
        }

        protected override byte[] CreateHttpContent()
        {
            using (var stream = new RequestBodyStream())
            {
                stream.WritePu16((byte)Operation.Rename);

                stream.WritePu32(00); // old revision
                stream.WriteString(_fromPath);

                stream.WritePu32(00); // new revision
                stream.WriteString(_toPath);

                stream.WritePu32(00); //dunno

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
                        OneRevision = Types.Revision.FromStream(data),
                        TwoRevision = Types.Revision.FromStream(data)
                    }
                };
            }

            throw new Exception($"{nameof(RenameRequest)} failed with result code {data.OperationResult}");
        }

        private const int Revision = 0;

        public class Result : BaseResponseResult
        {
            public Revision OneRevision;
            public Revision TwoRevision;
        }

}
}