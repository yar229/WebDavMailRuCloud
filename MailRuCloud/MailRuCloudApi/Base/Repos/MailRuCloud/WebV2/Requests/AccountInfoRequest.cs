using YaR.MailRuCloud.Api.Base.Requests;
using YaR.MailRuCloud.Api.Base.Requests.Types;

namespace YaR.MailRuCloud.Api.Base.Repos.MailRuCloud.WebV2.Requests
{
    class AccountInfoRequest : BaseRequestJson<AccountInfoRequestResult>
    {
        public AccountInfoRequest(HttpCommonSettings settings, IAuth auth) : base(settings, auth)
        {
        }

        protected override string RelationalUri => $"{ConstSettings.CloudDomain}/api/v2/user?token={Auth.AccessToken}";
    }
}
