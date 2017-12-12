using System;
using System.Net;

namespace YaR.MailRuCloud.Api.Base.Requests.WebBin
{
    internal class WeblinkGetServerRequest : BaseRequestString<WeblinkGetServerRequest.Result>
    {
        public WeblinkGetServerRequest(IWebProxy proxy) : base(proxy, null)
        {
        }

        protected override string RelationalUri => "https://dispatcher.cloud.mail.ru/y";

        protected override RequestResponse<Result> DeserializeMessage(string data)
        {
            var datas = data.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            var msg = new RequestResponse<Result>
            {
                Ok = true,
                Result = new Result
                {
                    Url = datas[0],
                    Ip = datas[1],
                    Unknown = int.Parse(datas[2])
                }
            };
            return msg;
        }

        public class Result
        {
            public string Url { get; set; }
            public string Ip { get; set; }
            public int Unknown { get; set; }
        }
    }
}