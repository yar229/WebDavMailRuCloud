using System.Net;
using YaR.MailRuCloud.Api.Base.Auth;

namespace YaR.MailRuCloud.Api.Base.Requests.WebM1
{
    class AccountInfoRequest : BaseRequestJson<WebV2.AccountInfoRequest.Result>
    {
        public AccountInfoRequest(HttpCommonSettings settings, IAuth auth) : base(settings, auth)
        {
        }

        protected override string RelationalUri => $"{ConstSettings.CloudDomain}/api/m1/user?access_token={Auth.AccessToken}";
    }
}
