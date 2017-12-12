using System;
using System.Net;
using System.Text;

namespace YaR.MailRuCloud.Api.Base.Requests.WebBin
{
    class OAuthSecondStepRequest : BaseRequestJson<OAuthRequest.Result>
    {
        private readonly string _clientId;
        private readonly string _login;
        private readonly string _tsaToken;
        private readonly string _authCode;

        public OAuthSecondStepRequest(IWebProxy proxy, string clientId, string login, string tsaToken, string authCode) : base(proxy, null)
        {
            _clientId = clientId;
            _login = login;
            _tsaToken = tsaToken;
            _authCode = authCode;
        }

        protected override string RelationalUri => "https://o2.mail.ru/token";

        protected override byte[] CreateHttpContent()
        {
            var data = $"client_id={_clientId}&grant_type=password&username={Uri.EscapeUriString(_login)}&tsa_token={_tsaToken}&auth_code={_authCode}";
            return Encoding.UTF8.GetBytes(data);
        }
    }
}