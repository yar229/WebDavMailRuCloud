using YaR.MailRuCloud.Api.Base.Auth;
using YaR.MailRuCloud.Api.Base.Requests.Types;

namespace YaR.MailRuCloud.Api.Base.Requests.WebV2
{
    class ShardInfoRequest : BaseRequestJson<ShardInfoRequestResult>
    {
        public ShardInfoRequest(HttpCommonSettings settings, IAuth auth) 
            : base(settings, auth)
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
