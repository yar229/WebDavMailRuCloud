namespace YaR.MailRuCloud.Api.Base.Requests.Mobile
{
    class AccountInfoRequest : BaseRequestJson<Web.AccountInfoRequest.Result>
    {
        public AccountInfoRequest(RequestInit init) : base(init)
        {
        }

        protected override string RelationalUri => $"{ConstSettings.CloudDomain}/api/m1/user?access_token={Init.Token}";
    }
}