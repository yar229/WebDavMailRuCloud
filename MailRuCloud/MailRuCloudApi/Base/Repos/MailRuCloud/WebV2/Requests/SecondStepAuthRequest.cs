using System;
using System.Text;
using YaR.Clouds.Base.Requests;

namespace YaR.Clouds.Base.Repos.MailRuCloud.WebV2.Requests
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

        protected override string RelationalUri => $"{CommonSettings.AuthDomain}/cgi-bin/secstep";

        protected override byte[] CreateHttpContent()
        {
            string data = $"csrf={_csrf}&Login={Uri.EscapeUriString(Auth.Login)}&AuthCode={_authCode}";

            return Encoding.UTF8.GetBytes(data);
        }
    }
}


