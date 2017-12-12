using System.Collections.Generic;
using System.Net;
using YaR.MailRuCloud.Api.Base.Requests.Repo;

namespace YaR.MailRuCloud.Api.Base.Requests.WebM1
{
    class ShardInfoRequest : BaseRequestJson<WebV2.ShardInfoRequest.Result>
    {
        public ShardInfoRequest(IWebProxy proxy, IAuth auth) : base(proxy, auth)
        {
        }

        protected override string RelationalUri
        {
            get
            {
                var uri = string.Format("{0}/api/m1/dispatcher?api=2", ConstSettings.CloudDomain);
                if (!string.IsNullOrEmpty(Auth.AccessToken))
                    uri += $"&access_token={Auth.AccessToken}";
                return uri;
            }
        }
    }
}
