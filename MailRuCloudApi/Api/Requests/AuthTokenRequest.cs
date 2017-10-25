using MailRuCloudApi.Api.Requests.Types;

namespace MailRuCloudApi.Api.Requests
{
    class AuthTokenRequest : BaseRequest<AuthTokenResult>
    {
        public AuthTokenRequest(CloudApi cloudApi) : base(cloudApi)
        {
        }

        public override string RelationalUri
        {
            get
            {
                const string uri = "/api/v2/tokens/csrf";
                return uri;
            }
        }
    }
}
