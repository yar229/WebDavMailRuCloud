using YaR.MailRuCloud.Api.Base.Requests.Types;

namespace YaR.MailRuCloud.Api.Base.Requests
{
    class AccountInfoRequest : BaseRequest<AccountInfoResult>
    {
        public AccountInfoRequest(CloudApi cloudApi) : base(cloudApi)
        {
        }

        public override string RelationalUri
        {
            get
            { 
                var uri = $"{ConstSettings.CloudDomain}/api/v2/user?token={CloudApi.Account.AuthToken}";
                return uri;
            }
        }
    }
}
