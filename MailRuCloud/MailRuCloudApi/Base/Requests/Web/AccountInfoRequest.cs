using YaR.MailRuCloud.Api.Base.Requests.Types;

namespace YaR.MailRuCloud.Api.Base.Requests.Web
{
    class AccountInfoRequest : BaseRequestJson<AccountInfoResult>
    {
        public AccountInfoRequest(CloudApi cloudApi) : base(cloudApi)
        {
        }

        protected override string RelationalUri => $"{ConstSettings.CloudDomain}/api/v2/user?token={CloudApi.Account.AuthToken}";
    }
}
