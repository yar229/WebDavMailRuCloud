using System;
using System.Net;
using YaR.MailRuCloud.Api.Base.Requests.Repo;

namespace YaR.MailRuCloud.Api.Base.Requests.WebV2
{
    class CloneItemRequest : BaseRequestJson<CloneItemRequest.Result>
    {
        private readonly string _fromUrl;
        private readonly string _toPath;

        public CloneItemRequest(HttpCommonSettings settings, IAuth auth, string fromUrl, string toPath) 
            : base(settings, auth)
        {
            _fromUrl = fromUrl;
            _toPath = toPath;
        }

        protected override string RelationalUri
        {
            get
            {
                var uri = $"{ConstSettings.CloudDomain}/api/v2/clone?conflict=rename&folder={Uri.EscapeDataString(_toPath)}&weblink={Uri.EscapeDataString(_fromUrl)}&token={Auth.AccessToken}";
                return uri;
            }
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
