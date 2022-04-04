using System;
using System.Collections.Specialized;
using YaR.Clouds.Base.Repos.MailRuCloud.Mobile.Requests.Types;
using YaR.Clouds.Base.Requests;

namespace YaR.Clouds.Base.Repos.MailRuCloud.Mobile.Requests
{
    class MoveRequest : BaseRequestMobile<MoveRequest.Result>
    {
        private readonly string _fromPath;
        private readonly string _toPath;

        public MoveRequest(HttpCommonSettings settings, IAuth auth, string metaServer, string fromPath, string toPath)
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

        protected override RequestResponse<Result> DeserializeMessage(NameValueCollection responseHeaders, ResponseBodyStream data)
        {
            var opres = (OpResult) (int) data.OperationResult;

            if (opres == OpResult.Ok)
            {
                return new RequestResponse<Result>
                {
                    Ok = true,
                    Result = new Result
                    {
                        OperationResult = data.OperationResult,
                        OneRevision = Revision.FromStream(data),
                        TwoRevision = Revision.FromStream(data)
                    }
                };
            }

            throw new Exception($"{nameof(MoveRequest)} failed with result code {opres}");
        }

        public class Result : BaseResponseResult
        {
            public Revision OneRevision;
            public Revision TwoRevision;
        }

        private enum OpResult
        {
            Ok = 0,
            SourceNotExists = 1,
            Failed002 = 2,
            AlreadyExists = 4,
            Failed005 = 5,
            Failed254 = 254
        }

    }
}