using System.Collections.Generic;
using System.Net;
using System.Net.Http;
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
            var keyValues = new List<KeyValuePair<string, string>>
            {
                new("csrf_token", _csrf),
                new("origin", "disk_landing2_web_signin_ru")
            };
            var content = new FormUrlEncodedContent(keyValues);
            var d = content.ReadAsByteArrayAsync().Result;
            return d;
        }
    }

    class YadAuthAccountsRequestResult
    {
        public bool HasError => Status == "error" ||
                                Accounts == null;

        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("csrf")]
        public string Csrf { get; set; }

        [JsonProperty("accounts")]
        public YadAccounts Accounts { get; set; }
    }

    class YadAccounts
    {
        [JsonProperty("authorizedAccountsDefaultUid")]
        public string AuthorizedAccountsDefaultUid { get; set; }
    }
}