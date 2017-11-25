using System;
using YaR.MailRuCloud.Api.Base.Requests.Types;

namespace YaR.MailRuCloud.Api.Base.Requests.Web
{
    class CloneItemRequest : BaseRequestJson<StatusResult>
    {
        private readonly string _fromUrl;
        private readonly string _toPath;

        public CloneItemRequest(CloudApi cloudApi, string fromUrl, string toPath) : base(cloudApi)
        {
            _fromUrl = fromUrl;
            _toPath = toPath;
        }

        protected override string RelationalUri
        {
            get
            {
                var uri = $"{ConstSettings.CloudDomain}/api/v2/clone?conflict=rename&folder={Uri.EscapeDataString(_toPath)}&weblink={Uri.EscapeDataString(_fromUrl)}&token={CloudApi.Account.AuthToken}";
                return uri;
            }
        }
    }
}
