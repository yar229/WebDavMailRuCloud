using Newtonsoft.Json;
using YaR.MailRuCloud.Api.Base.Auth;
using YaR.MailRuCloud.Api.Base.Requests.Types;

namespace YaR.MailRuCloud.Api.Base.Requests.WebV2
{
    class AuthTokenRequest : BaseRequestJson<AuthTokenRequest.Result>
    {
        public AuthTokenRequest(HttpCommonSettings settings, IAuth auth) : base(settings, auth)
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

        public class Result : CommonOperationResult<Result.AuthTokenResultBody>
        {
            public class AuthTokenResultBody
            {
                [JsonProperty("token")]
                public string Token { get; set; }
            }
        }


    }
}
