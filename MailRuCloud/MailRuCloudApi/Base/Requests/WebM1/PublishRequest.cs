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

        protected override string RelationalUri => $"/api/m1/file/publish?access_token={Auth.AccessToken}";

        protected override byte[] CreateHttpContent()
        {
            var data = $"home={Uri.EscapeDataString(_fullPath)}&email={Auth.Login}&x-email={Auth.Login}";
            return Encoding.UTF8.GetBytes(data);
        }
    }
}