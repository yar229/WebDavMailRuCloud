using System;
using System.Text;
using YaR.MailRuCloud.Api.Base.Requests.Web;

namespace YaR.MailRuCloud.Api.Base.Requests.Mobile
{
    public class MobMetaServerRequest : BaseRequest<MobMetaServerRequest.Result>
    {
        public MobMetaServerRequest(CloudApi cloudApi) : base(cloudApi)
        {
        }

        protected override string RelationalUri => "https://dispatcher.cloud.mail.ru/m";

        protected override RequestResponse<Result> DeserializeMessage(string json)
        {
            var data = json.Split(new []{' '}, StringSplitOptions.RemoveEmptyEntries);
            var msg = new RequestResponse<Result>
            {
                Ok = true,
                Result = new Result
                {
                    Url = data[0],
                    Ip = data[1],
                    Unknown = int.Parse(data[2])
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