using System;
using YaR.MailRuCloud.Api.Base.Requests.Types;

namespace YaR.MailRuCloud.Api.Base.Requests
{
    class CloneItemRequest : BaseRequest<CloneItemResult>
    {
        private readonly string _fromUrl;
        private readonly string _toPath;

        public CloneItemRequest(CloudApi cloudApi, string fromUrl, string toPath) : base(cloudApi)
        {
            _fromUrl = fromUrl;
            _toPath = toPath;
        }

        public override string RelationalUri
        {
            get
            {
                var uri = $"{ConstSettings.CloudDomain}/api/v2/clone?folder={Uri.EscapeDataString(_toPath)}&weblink={_fromUrl}&token={CloudApi.Account.AuthToken}";
                return uri;
            }
        }
    }
}
