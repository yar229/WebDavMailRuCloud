using System.Collections.Generic;
using System.Net;
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
                var uri = $"{ConstSettings.CloudDomain}/api/v2/dispatcher?client_id={Settings.ClientId}";
                if (!Auth.IsAnonymous)
                    uri += $"&access_token={Auth.AccessToken}";
                else
                {
                    uri += "&email=anonym&x-email=anonym";
                }
                return uri;
            }
        }
    }
}
