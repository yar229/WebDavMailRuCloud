using YaR.MailRuCloud.Api.Base.Auth;
using YaR.MailRuCloud.Api.Base.Requests.Types;

namespace YaR.MailRuCloud.Api.Base.Requests.WebV2
{
    class AccountInfoRequest : BaseRequestJson<AccountInfoRequestResult>
    {
        public AccountInfoRequest(HttpCommonSettings settings, IAuth auth) : base(settings, auth)
        {
        }

        protected override string RelationalUri => $"{ConstSettings.CloudDomain}/api/v2/user?token={Auth.AccessToken}";
    }
}
