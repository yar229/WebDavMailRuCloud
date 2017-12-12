using System;
using System.Net;
using System.Text;

namespace YaR.MailRuCloud.Api.Base.Requests.Mobile
{
    class OAuthRequest: BaseRequestJson<OAuthRequest.Result>
    {
        private readonly string _clientId;
        private readonly string _login;
        private readonly string _password;

        public OAuthRequest(IWebProxy proxy, IBasicCredentials creds, string clientId) : base(proxy, null)
        {
            _clientId = clientId;
            _login = Uri.EscapeDataString(creds.Login);
            _password = Uri.EscapeDataString(creds.Password);
        }

        protected override string RelationalUri => "https://o2.mail.ru/token";

        protected override byte[] CreateHttpContent()
        {
            var data = $"password={_password}&client_id={_clientId}&username={_login}&grant_type=password";
            return Encoding.UTF8.GetBytes(data);
        }


        public class Result
        {
            public int expires_in { get; set; }
            public string refresh_token { get; set; }
            public string access_token { get; set; }

            public string error { get; set; }
            public int error_code { get; set; }
            public string error_description { get; set; }

            /// <summary>
            /// Token for second step auth
            /// </summary>
            public string tsa_token { get; set; }
            /// <summary>
            /// Code length for second step auth
            /// </summary>
            public int length { get; set; }
            /// <summary>
            /// Seconds to wait for for second step auth code
            /// </summary>
            public int timeout { get; set; }
        }
    }
}
