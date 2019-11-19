using YaR.MailRuCloud.Api.Base.Auth;
using YaR.MailRuCloud.Api.Base.Requests.Types;

namespace YaR.MailRuCloud.Api.Base.Requests.WebM1
{
    class AccountInfoRequest : BaseRequestJson<AccountInfoRequestResult>
    {
        public AccountInfoRequest(HttpCommonSettings settings, IAuth auth) : base(settings, auth)
        {
        }

        protected override string RelationalUri => $"{ConstSettings.CloudDomain}/api/m1/user?access_token={Auth.AccessToken}";
    }
}
