using System;
using System.Net;
using System.Text;
using YaR.MailRuCloud.Api.Base.Requests.Repo;

namespace YaR.MailRuCloud.Api.Base.Requests.WebM1
{
    class PublishRequest : BaseRequestJson<WebV2.PublishRequest.Result>
    {
        private readonly string _fullPath;

        public PublishRequest(IWebProxy proxy, IAuth auth, string fullPath) : base(proxy, auth)
        {
            _fullPath = fullPath;
        }

        protected override string RelationalUri => "/api/m1/file/publish";

        protected override byte[] CreateHttpContent()
        {
            var data = string.Format("home={0}&api={1}&access_token={2}&email={3}&x-email={3}", Uri.EscapeDataString(_fullPath),
                2, Auth.AccessToken, Auth.Login);
            return Encoding.UTF8.GetBytes(data);
        }
    }
}