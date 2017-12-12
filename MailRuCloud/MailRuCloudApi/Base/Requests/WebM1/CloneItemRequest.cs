using System;
using System.Net;
using YaR.MailRuCloud.Api.Base.Requests.Repo;

namespace YaR.MailRuCloud.Api.Base.Requests.WebM1
{
    class CloneItemRequest : BaseRequestJson<WebV2.CloneItemRequest.Result>
    {
        private readonly string _fromUrl;
        private readonly string _toPath;

        public CloneItemRequest(IWebProxy proxy, IAuth auth, string fromUrl, string toPath) 
            : base(proxy, auth)
        {
            _fromUrl = fromUrl;
            _toPath = toPath;
        }

        protected override string RelationalUri
        {
            get
            {
                var uri = $"{ConstSettings.CloudDomain}/api/m1/clone?conflict=rename&folder={Uri.EscapeDataString(_toPath)}&weblink={Uri.EscapeDataString(_fromUrl)}&access_token={Auth.AccessToken}";
                return uri;
            }
        }
    }
}
