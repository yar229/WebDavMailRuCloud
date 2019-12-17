using Newtonsoft.Json;

namespace YaR.MailRuCloud.Api.Base.Requests.Types
{
    class AuthTokenRequestResult : CommonOperationResult<AuthTokenRequestResult.AuthTokenResultBody>
    {
        public class AuthTokenResultBody
        {
            [JsonProperty("token")]
            public string Token { get; set; }
        }
    }
}
