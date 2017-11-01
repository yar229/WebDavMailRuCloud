using YaR.MailRuCloud.Api.Base.Requests.Types;

namespace YaR.MailRuCloud.Api.Base.Requests
{
    class AuthTokenRequest : BaseRequest<AuthTokenResult>
    {
        public AuthTokenRequest(CloudApi cloudApi) : base(cloudApi)
        {
        }

        protected override string RelationalUri
        {
            get
            {
                const string uri = "/api/v2/tokens/csrf";
                return uri;
            }
        }
    }
}
