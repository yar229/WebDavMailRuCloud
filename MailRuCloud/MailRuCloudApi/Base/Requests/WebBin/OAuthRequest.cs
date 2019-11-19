using System;
using System.Text;
using Newtonsoft.Json;

namespace YaR.MailRuCloud.Api.Base.Requests.WebBin
{
    class OAuthRequest: BaseRequestJson<OAuthRequest.Result>
    {
        private readonly string _login;
        private readonly string _password;

        public OAuthRequest(HttpCommonSettings settings, IBasicCredentials creds) : base(settings, null)
        {
            _login = Uri.EscapeDataString(creds.Login);
            _password = Uri.EscapeDataString(creds.Password);
        }

        protected override string RelationalUri => "https://o2.mail.ru/token";

        protected override byte[] CreateHttpContent()
        {
            var data = $"password={_password}&client_id={Settings.ClientId}&username={_login}&grant_type=password";
            return Encoding.UTF8.GetBytes(data);
        }


        public class Result
        {
            [JsonProperty("expires_in")]
            public int ExpiresIn { get; set; }
            [JsonProperty("refresh_token")]
            public string RefreshToken { get; set; }
            [JsonProperty("access_token")]
            public string AccessToken { get; set; }

            [JsonProperty("error")]
            public string Error { get; set; }
            [JsonProperty("error_code")]
            public int ErrorCode { get; set; }
            [JsonProperty("error_description")]
            public string ErrorDescription { get; set; }

            /// <summary>
            /// Token for second step auth
            /// </summary>
            [JsonProperty("tsa_token")]
            public string TsaToken { get; set; }
            /// <summary>
            /// Code length for second step auth
            /// </summary>
            [JsonProperty("length")]
            public int Length { get; set; }
            /// <summary>
            /// Seconds to wait for for second step auth code
            /// </summary>
            [JsonProperty("timeout")]
            public int Timeout { get; set; }
        }
    }
}
