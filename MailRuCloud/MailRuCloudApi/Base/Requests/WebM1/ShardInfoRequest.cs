using YaR.MailRuCloud.Api.Base.Auth;

namespace YaR.MailRuCloud.Api.Base.Requests.WebM1
{
    class ShardInfoRequest : BaseRequestJson<WebV2.ShardInfoRequest.Result>
    {
        public ShardInfoRequest(HttpCommonSettings settings, IAuth auth) : base(settings, auth)
        {
        }

        protected override string RelationalUri
        {
            get
            {
                var uri = $"{ConstSettings.CloudDomain}/api/m1/dispatcher?client_id={Settings.ClientId}";
                if (!string.IsNullOrEmpty(Auth.AccessToken))
                    uri += $"&access_token={Auth.AccessToken}";
                return uri;
            }
        }
    }
}
