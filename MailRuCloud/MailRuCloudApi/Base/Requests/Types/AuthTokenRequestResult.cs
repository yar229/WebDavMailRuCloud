using Newtonsoft.Json;

namespace YaR.Clouds.Base.Requests.Types
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
