namespace MailRuCloudApi.Api.Requests
{
    class AccountInfoRequest : BaseRequest<Types.AccountInfoResult>
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
