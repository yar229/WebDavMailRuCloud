using System.Net;
using YaR.MailRuCloud.Api.Base.Requests.Repo;

namespace YaR.MailRuCloud.Api.Base.Requests
{
    internal abstract class BaseRequestString : BaseRequestString<string>
    {
        protected BaseRequestString(IWebProxy proxy, IAuth auth) : base(proxy, auth)
        {
        }

        protected override RequestResponse<string> DeserializeMessage(string data)
        {
            var msg = new RequestResponse<string>
            {
                Ok = true,
                Result = data
            };
            return msg;
        }
    }
}