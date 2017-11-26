using System;

namespace YaR.MailRuCloud.Api.Base.Requests.Web
{
    class CloneItemRequest : BaseRequestJson<CloneItemRequest.Result>
    {
        private readonly string _token;
        private readonly string _fromUrl;
        private readonly string _toPath;

        public CloneItemRequest(CloudApi cloudApi, string token, string fromUrl, string toPath) : base(cloudApi)
        {
            _token = token;
            _fromUrl = fromUrl;
            _toPath = toPath;
        }

        protected override string RelationalUri
        {
            get
            {
                var uri = $"{ConstSettings.CloudDomain}/api/v2/clone?conflict=rename&folder={Uri.EscapeDataString(_toPath)}&weblink={Uri.EscapeDataString(_fromUrl)}&token={_token}";
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
