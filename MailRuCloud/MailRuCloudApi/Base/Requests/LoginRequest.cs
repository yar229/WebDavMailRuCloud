using System;
using System.Linq;
using System.Net;
using System.Text;
using YaR.MailRuCloud.Api.Base.Requests.Types;

namespace YaR.MailRuCloud.Api.Base.Requests
{
    class LoginRequest : BaseRequest<LoginResult>
    {
        private readonly string _login;
        private readonly string _password;

        public LoginRequest(CloudApi cloudApi, string login, string password) : base(cloudApi)
        {
            _login = login;
            _password = password;
        }

        protected override HttpWebRequest CreateRequest(string baseDomain = null)
        {
            var request = base.CreateRequest(ConstSettings.AuthDomain);
            request.Accept = ConstSettings.DefaultAcceptType;
            return request;
        }

        protected override string RelationalUri => "/cgi-bin/auth";

        protected override byte[] CreateHttpContent()
        {
            string data = $"Login={Uri.EscapeUriString(_login)}&Domain={ConstSettings.Domain}&Password={Uri.EscapeUriString(_password)}";

            return Encoding.UTF8.GetBytes(data);
        }

        protected override RequestResponse<LoginResult> DeserializeMessage(string responseText)
        {
            var csrf = responseText.Contains("csrf")
                ? new string(responseText.Split(new[] {"csrf"}, StringSplitOptions.None)[1].Split(',')[0].Where(char.IsLetterOrDigit).ToArray())
                : string.Empty;

            var msg = new RequestResponse<LoginResult>
            {
                Ok = true,
                Result = new LoginResult
                {
                    Csrf = csrf
                }
            };
            return msg;
        }
    }
}
