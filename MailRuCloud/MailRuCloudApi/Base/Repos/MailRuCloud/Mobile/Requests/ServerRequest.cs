using System;
using YaR.Clouds.Base.Requests;
using YaR.Clouds.Base.Requests.Types;

namespace YaR.Clouds.Base.Repos.MailRuCloud.Mobile.Requests
{
    internal abstract class ServerRequest : BaseRequestString<ServerRequestResult>
    {
        protected ServerRequest(HttpCommonSettings settings) : base(settings, null)
        {
        }

        protected override RequestResponse<ServerRequestResult> DeserializeMessage(string data)
        {
            var datas = data.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            var msg = new RequestResponse<ServerRequestResult>
            {
                Ok = true,
                Result = new ServerRequestResult
                {
                    Url = datas[0],
                    Ip = datas[1],
                    Unknown = int.Parse(datas[2])
                }
            };
            return msg;
        }
    }
}