using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Security.Authentication;
using Newtonsoft.Json;
using YaR.Clouds.Base.Requests;

namespace YaR.Clouds.Base.Repos.YandexDisk.YadWebV2.Requests
{
    class YadAuthPasswordRequest : BaseRequestJson<YadAuthPasswordRequestResult>
    {
        private readonly IAuth _auth;
        private readonly string _csrf;
        private readonly string _trackId;

        private string secChUa;

        public YadAuthPasswordRequest(HttpCommonSettings settings, IAuth auth, string csrf, string trackId) 
            : base(settings, auth)
        {
            _auth = auth;
            _csrf = csrf;
            _trackId = trackId;

            secChUa = settings.CloudSettings.SecChUa;
        }

        protected override string RelationalUri => "/registration-validations/auth/multi_step/commit_password";

        protected override HttpWebRequest CreateRequest(string baseDomain = null)
        {
            var request = base.CreateRequest("https://passport.yandex.ru");

            request.Accept = "application/json, text/javascript, */*; q=0.01";
            request.Referer = "https://passport.yandex.ru/";
            request.ContentType = "application/x-www-form-urlencoded; charset=UTF-8";

            // Строка вида "\" Not A; Brand\";v=\"99\", \"Chromium\";v=\"99\", \"Google Chrome\";v=\"99\""
            request.Headers.Add("sec-ch-ua", secChUa);
            request.Headers.Add("X-Requested-With", "XMLHttpRequest");
            request.Headers.Add("sec-ch-ua-mobile", "?0");
            request.Headers.Add("sec-ch-ua-platform", "\"Windows\"");
            request.Headers.Add("Origin", "https://passport.yandex.ru");
            request.Headers.Add("Sec-Fetch-Site", "same-origin");
            request.Headers.Add("Sec-Fetch-Mode", "cors");
            request.Headers.Add("Sec-Fetch-Dest", "empty");
            request.Headers.Add("Accept-Encoding", "gzip, deflate, br");
            request.Headers.Add("Accept-Language", "ru-RU,ru;q=0.9,en-US;q=0.8,en;q=0.7,es;q=0.6");

            return request;
        }

        protected override byte[] CreateHttpContent()
        {
            var keyValues = new List<KeyValuePair<string, string>>
            {
                new("csrf_token", Uri.EscapeDataString(_csrf)),
                new("track_id", _trackId),
                new("password", Uri.EscapeDataString(_auth.Password)),
                new("retpath", Uri.EscapeDataString("https://disk.yandex.ru/client/disk"))
            };
            var content = new FormUrlEncodedContent(keyValues);
            var d = content.ReadAsByteArrayAsync().Result;
            return d;
        }

        protected override RequestResponse<YadAuthPasswordRequestResult> DeserializeMessage(NameValueCollection responseHeaders, Stream stream)
        {
            var res = base.DeserializeMessage(responseHeaders, stream);

            if (res.Result.State == "auth_challenge")
                throw new AuthenticationException("Browser login required to accept additional confirmations");

            var uid = responseHeaders["X-Default-UID"];
            if (string.IsNullOrWhiteSpace(uid))
                throw new AuthenticationException("Cannot get X-Default-UID");
            res.Result.DefaultUid = uid;

            return res;
        }
    }

    class YadAuthPasswordRequestResult
    {
        public bool HasError => Status == "error";

        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("state")]
        public string State { get; set; }

        [JsonProperty("retpath")]
        public string RetPath { get; set; }

        [JsonIgnore]
        public string DefaultUid { get; set; }

        [JsonProperty("errors")]
        public List<string> Errors { get; set; }
    }
}