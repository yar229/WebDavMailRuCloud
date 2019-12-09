using YaR.MailRuCloud.Api.Base.Requests;
using YaR.MailRuCloud.Api.Base.Requests.Types;

namespace YaR.MailRuCloud.Api.Base.Repos.WebM1.Requests
{
    class ShardInfoRequest : BaseRequestJson<ShardInfoRequestResult>
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
