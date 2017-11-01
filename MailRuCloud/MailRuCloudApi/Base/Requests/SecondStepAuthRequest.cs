using System;
using System.Text;

namespace YaR.MailRuCloud.Api.Base.Requests
{
    class SecondStepAuthRequest : BaseRequest<string>
    {
        private readonly string _csrf;
        private readonly string _login;
        private readonly string _authCode;

        public SecondStepAuthRequest(CloudApi cloudApi, string csrf, string login, string authCode) : base(cloudApi)
        {
            _csrf = csrf;
            _login = login;
            _authCode = authCode;
        }

        protected override string RelationalUri => $"{ConstSettings.AuthDomain}/cgi-bin/secstep";

        protected override byte[] CreateHttpContent()
        {
            string data = $"csrf={_csrf}&Login={Uri.EscapeUriString(_login)}&AuthCode={_authCode}";

            return Encoding.UTF8.GetBytes(data);
        }
    }
}


