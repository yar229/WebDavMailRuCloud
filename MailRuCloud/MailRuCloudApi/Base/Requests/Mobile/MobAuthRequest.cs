using System;
using System.Text;

namespace YaR.MailRuCloud.Api.Base.Requests.Mobile
{
    class MobAuthRequest: BaseRequestJson<MobAuthRequest.Result>
    {
        private readonly string _login;
        private readonly string _password;

        public MobAuthRequest(CloudApi cloudApi, string login, string password) : base(cloudApi)
        {
            _login = Uri.EscapeDataString(login);
            _password = Uri.EscapeDataString(password);
        }

        protected override string RelationalUri => "https://o2.mail.ru/token";

        protected override byte[] CreateHttpContent()
        {
            var data = $"password={_password}&client_id=cloud-android&username={_login}&grant_type=password";
            return Encoding.UTF8.GetBytes(data);
        }


        public class Result
        {
            public int expires_in { get; set; }
            public string refresh_token { get; set; }
            public string access_token { get; set; }
        }
    }
}
