using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using Newtonsoft.Json;
using YaR.Clouds.Base.Requests;

namespace YaR.Clouds.Base.Repos.YandexDisk.YadWeb.Requests
{
    class YadAuthLoginRequest : BaseRequestJson<YadAuthLoginRequestResult>
    {
        private readonly IAuth _auth;
        private readonly string _csrf;
        private readonly string _uuid;

        public YadAuthLoginRequest(HttpCommonSettings settings, IAuth auth, string csrf, string uuid) 
            : base(settings, auth)
        {
            _auth = auth;
            _csrf = csrf;
            _uuid = uuid;
        }

        protected override string RelationalUri => "/registration-validations/auth/multi_step/start";

        protected override HttpWebRequest CreateRequest(string baseDomain = null)
        {
            var request = base.CreateRequest("https://passport.yandex.ru");
            request.Referer = "https://passport.yandex.ru/auth/list?from=cloud&origin=disk_landing_web_signin_ru&retpath=https%3A%2F%2Fdisk.yandex.ru%2Fclient%2Fdisk&backpath=https%3A%2F%2Fdisk.yandex.ru&mode=edit";
            request.Headers["Sec-Fetch-Mode"] = "cors";
            request.Headers["Sec-Fetch-Site"] = "same-origin";

            return request;
        }

        protected override byte[] CreateHttpContent()
        {
            
            var keyValues = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("csrf_token", _csrf),
                new KeyValuePair<string, string>("process_uuid", _uuid),
                new KeyValuePair<string, string>("login", _auth.Login),
                new KeyValuePair<string, string>("service", "cloud"),
                new KeyValuePair<string, string>("retpath", "https://disk.yandex.ru?source=landing2_signin_ru"),
                new KeyValuePair<string, string>("origin", "disk_landing2_signin_ru")
            };
            FormUrlEncodedContent z = new FormUrlEncodedContent(keyValues);
            var d = z.ReadAsByteArrayAsync().Result;
            return d;
        }
    }

    class YadAuthLoginRequestResult
    {
        public bool HasError => Status == "error";

        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("track_id")]
        public string TrackId { get; set; }



        [JsonProperty("errors")]
        public List<string> Errors { get; set; }
    }
}