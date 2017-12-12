using System;
using System.Net;
using System.Text;
using YaR.MailRuCloud.Api.Base.Requests.Repo;

namespace YaR.MailRuCloud.Api.Base.Requests.WebM1
{
    class UnpublishRequest : BaseRequestJson<WebV2.UnpublishRequest.Result>
    {
        private readonly string _publicLink;

        public UnpublishRequest(IWebProxy proxy, IAuth auth, string publicLink) : base(proxy, auth)
        {
            _publicLink = publicLink;
        }

        protected override string RelationalUri => $"/api/m1/file/unpublish?access_token={Auth.AccessToken}";

        protected override byte[] CreateHttpContent()
        {
            var data = $"weblink={Uri.EscapeDataString(_publicLink)}&email={Auth.Login}&x-email={Auth.Login}";
            return Encoding.UTF8.GetBytes(data);
        }
    }
}