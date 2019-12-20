using System.Net;
using System.Text;
using Newtonsoft.Json;
using YaR.Clouds.Base.Requests;

namespace YaR.Clouds.Base.Repos.MailRuCloud.Mobile.Requests
{
    class OAuthRefreshRequest : BaseRequestJson<OAuthRefreshRequest.Result>
    {
        private readonly string _refreshToken;

        public OAuthRefreshRequest(HttpCommonSettings settings, string refreshToken) : base(settings, null)
        {
            _refreshToken = refreshToken;
        }

        protected override string RelationalUri => "https://o2.mail.ru/token";

        protected override byte[] CreateHttpContent()
        {
            var data = $"client_id={Settings.ClientId}&grant_type=refresh_token&refresh_token={_refreshToken}";
            return Encoding.UTF8.GetBytes(data);
        }

        protected override HttpWebRequest CreateRequest(string baseDomain = null)
        {
            var request = base.CreateRequest(baseDomain);
            request.Host = request.RequestUri.Host;
            request.UserAgent = Settings.UserAgent;
            request.Accept = "*/*";
            request.ServicePoint.Expect100Continue = false;

            return request;
        }


        public class Result
        {
            [JsonProperty("access_token")]
            public string AccessToken { get; set; }
            [JsonProperty("expires_in")]
            public int ExpiresIn { get; set; }

            [JsonProperty("error")]
            public string Error { get; set; }
            [JsonProperty("error_code")]
            public int ErrorCode { get; set; }
            [JsonProperty("error_description")]
            public string ErrorDescription { get; set; }
        }
    }
}