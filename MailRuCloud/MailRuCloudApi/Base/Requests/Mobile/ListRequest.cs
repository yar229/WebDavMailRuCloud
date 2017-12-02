using System;
using System.Collections.Generic;

namespace YaR.MailRuCloud.Api.Base.Requests.Mobile
{
    class ListRequest : BaseRequestMobile<ListRequest.Result>
    {
        private readonly string _fullPath;

        public ListRequest(RequestInit init, string metaServer, string fullPath)
            : base(init, metaServer)
        {
            _fullPath = fullPath;
        }

        protected override byte[] CreateHttpContent()
        {
            using (var stream = new RequestBodyStream())
            {
                stream.WritePu16((byte)Operation.FolderList);
                stream.WriteString(_fullPath);
                stream.WritePu32(_longAlways1Unknown);

                Int32 mask = 0;
                mask |= 1;
                mask |= 64;
                mask |= 128;
                mask |= 256;
                mask |= 32;
                stream.WritePu32(mask);

                stream.WriteWithLength(new byte[0]);


                var body = stream.GetBytes();
                return body;
            }
        }

        protected override RequestResponse<Result> DeserializeMessage(ResponseBodyStream data)
        {
            if (data.OperationResult != OperationResult.Ok)
                throw new Exception($"{nameof(ListRequest)} failed with result code {data.OperationResult}");

            var zzz = data.ReadAllBytes();

            var res = new Result {OperationResult = data.OperationResult};

            return new RequestResponse<Result>
            {
                Ok = data.OperationResult == OperationResult.Ok,
                Result = res
            };
        }

        //private FsItem ParseItem(string path, ResponseBodyStream body)
        //{
        //    int revision = body.ReadShort();

        //    int nodeId = body.ReadIntSpl();
        //    byte[] bytes16 = null;
        //    if ((nodeId & 4096) != 0)
        //        bytes16 = body.ReadNBytes(16);

        //    body.ReadBigNumber(); //dunno

        //    int opresult = nodeId & 3;

        //    FsItem item;
        //    switch (opresult)
        //    {
        //        case 0:
                    
        //            break;
        //        case 1:
                    
        //            break;
        //        case 2:
                    
        //            break;
        //        case 3:

        //            break;
        //        default:
        //            throw new Exception($"{nameof(ParseItem)} failed with code {opresult}");
        //    }

        //    return item;
        //}


        private const long _longAlways1Unknown = 1;
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