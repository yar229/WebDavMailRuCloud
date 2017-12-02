using System;
using System.Collections.Generic;

namespace YaR.MailRuCloud.Api.Base.Requests.Mobile
{
    class MobItemInfoRequest : BaseRequestMobile<MobItemInfoRequest.Result>
    {
        private readonly string[] _fullPath;

        public MobItemInfoRequest(RequestInit init, string metaServer, string[] fullPath)
            : base(init, metaServer)
        {
            _fullPath = fullPath;
        }

        protected override byte[] CreateHttpContent()
        {
            using (var stream = new RequestBodyStream())
            {
                stream.WritePu16((byte)Operation.ItemInfo);
                foreach (var path in _fullPath)
                {
                    stream.WriteString(path);
                    stream.WritePu32(00);
                }
                var body = stream.GetBytes();
                return body;
            }
        }

        protected override RequestResponse<Result> DeserializeMessage(ResponseBodyStream data)
        {
            if (data.OperationResult != OperationResult.Ok)
                throw new Exception($"{nameof(MobItemInfoRequest)} failed with result code {data.OperationResult}");

            var res = new Result {OperationResult = data.OperationResult};

            foreach (string path in _fullPath)
            {
                res.Items.Add(ParseItem(path, data));
            }

            return new RequestResponse<Result>
            {
                Ok = data.OperationResult == OperationResult.Ok,
                Result = res
            };
        }

        private FsItem ParseItem(string path, ResponseBodyStream body)
        {
            int revision = body.ReadShort();

            int nodeId = body.ReadIntSpl();
            byte[] bytes16 = null;
            if ((nodeId & 4096) != 0)
                bytes16 = body.ReadNBytes(16);

            body.ReadBigNumber(); //dunno

            int opresult = nodeId & 3;

            FsItem item;
            switch (opresult)
            {
                case 0:
                    
                    break;
                case 1:
                    
                    break;
                case 2:
                    
                    break;
                case 3:

                    break;
                default:
                    throw new Exception($"{nameof(ParseItem)} failed with code {opresult}");
            }

            return item;
        }



        private const int Revision = 0;

        public class Result : BaseResponseResult
        {
            public List<FsItem> Items { get; set; }
        }

        public class FsItem
        {
            public short Revisiona { get; set; }
        }
    }
}