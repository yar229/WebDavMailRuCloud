using System;
using System.Text;

namespace YaR.MailRuCloud.Api.Base.Requests.Web
{
    class SecondStepAuthRequest : BaseRequestString
    {
        private readonly string _csrf;
        private readonly string _authCode;

        public SecondStepAuthRequest(RequestInit init, string csrf, string authCode) : base(init)
        {
            _csrf = csrf;
            _authCode = authCode;
        }

        protected override string RelationalUri => $"{ConstSettings.AuthDomain}/cgi-bin/secstep";

        protected override byte[] CreateHttpContent()
        {
            string data = $"csrf={_csrf}&Login={Uri.EscapeUriString(Init.Login)}&AuthCode={_authCode}";

            return Encoding.UTF8.GetBytes(data);
        }
    }
}


