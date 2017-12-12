using System.Net;
using System.Text;

namespace YaR.MailRuCloud.Api.Base.Requests.Mobile
{
    class OAuthRefreshRequest : BaseRequestJson<OAuthRefreshRequest.Result>
    {
        private readonly string _clientId;
        private readonly string _refreshToken;

        public OAuthRefreshRequest(IWebProxy proxy, string clientId, string refreshToken) : base(proxy, null)
        {
            _clientId = clientId;
            _refreshToken = refreshToken;
        }

        protected override string RelationalUri => "https://o2.mail.ru/token";

        protected override byte[] CreateHttpContent()
        {
            var data = $"client_id={_clientId}&grant_type=refresh_token&refresh_token={_refreshToken}";
            return Encoding.UTF8.GetBytes(data);
        }

        protected override HttpWebRequest CreateRequest(string baseDomain = null)
        {
            var request = base.CreateRequest(baseDomain);
            request.Host = request.RequestUri.Host;
            request.UserAgent = ConstSettings.UserAgent;
            request.Accept = "*/*";
            request.ServicePoint.Expect100Continue = false;

            return request;
        }


        public class Result
        {
            public string access_token { get; set; }
            public int expires_in { get; set; }
        }
    }
}