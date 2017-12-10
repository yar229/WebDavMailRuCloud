using System.Net;
using YaR.MailRuCloud.Api.Base.Requests.Repo;

namespace YaR.MailRuCloud.Api.Base.Requests.Mobile
{
    class AccountInfoRequest : BaseRequestJson<Web.AccountInfoRequest.Result>
    {
        public AccountInfoRequest(IWebProxy proxy, IAuth auth) : base(proxy, auth)
        {
        }

        protected override string RelationalUri => $"{ConstSettings.CloudDomain}/api/m1/user?access_token={Auth.AccessToken}";
    }
}