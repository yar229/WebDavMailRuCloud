using System.Net;
using System.Text;
using Newtonsoft.Json;
using YaR.Clouds.Base.Requests;

namespace YaR.Clouds.Base.Repos.YandexDisk.YadWeb.Requests
{
    class YadAuthAccountsRequest : BaseRequestJson<YadAuthAccountsRequestResult>
    {
        private readonly string _csrf;

        public YadAuthAccountsRequest(HttpCommonSettings settings, IAuth auth, string csrf) 
            : base(settings, auth)
        {
            _csrf = csrf;
        }

        protected override string RelationalUri => "/registration-validations/auth/accounts";

        protected override HttpWebRequest CreateRequest(string baseDomain = null)
        {
            var request = base.CreateRequest("https://passport.yandex.ru");
            return request;
        }

        protected override byte[] CreateHttpContent()
        {
            var data = Encoding.UTF8.GetBytes($"csrf_token={_csrf}");
            return data;
        }
    }

    class YadAuthAccountsRequestResult
    {
        public bool HasError => Status == "error";

        [JsonProperty("status")]
        public string Status { get; set; }
    }
}