using System;
using System.Net;
using System.Text;
using YaR.MailRuCloud.Api.Base.Requests.Repo;

namespace YaR.MailRuCloud.Api.Base.Requests.Web
{
    class UnpublishRequest : BaseRequestJson<UnpublishRequest.Result>
    {
        private readonly string _publicLink;

        public UnpublishRequest(IWebProxy proxy, IAuth auth, string publicLink) : base(proxy, auth)
        {
            _publicLink = publicLink;
        }

        protected override string RelationalUri => "/api/v2/file/unpublish";

        protected override byte[] CreateHttpContent()
        {
            var data = string.Format("weblink={0}&api={1}&token={2}&email={3}&x-email={3}", Uri.EscapeDataString(_publicLink),
                2, Auth.AccessToken, Auth.Login);
            return Encoding.UTF8.GetBytes(data);
        }

        public class Result
        {
            public string email { get; set; }
            public string body { get; set; }
            public long time { get; set; }
            public int status { get; set; }
        }
    }
}