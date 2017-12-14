using System;
using System.Net;
using System.Text;

namespace YaR.MailRuCloud.Api.Base.Requests.WebV2
{
    class SecondStepAuthRequest : BaseRequestString
    {
        private readonly string _csrf;
        private readonly string _authCode;

        public SecondStepAuthRequest(HttpCommonSettings settings, string csrf, string authCode) : base(settings, null)
        {
            _csrf = csrf;
            _authCode = authCode;
        }

        protected override string RelationalUri => $"{ConstSettings.AuthDomain}/cgi-bin/secstep";

        protected override byte[] CreateHttpContent()
        {
            string data = $"csrf={_csrf}&Login={Uri.EscapeUriString(Auth.Login)}&AuthCode={_authCode}";

            return Encoding.UTF8.GetBytes(data);
        }
    }
}


