using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Security.Authentication;
using Newtonsoft.Json;
using YaR.Clouds.Base.Requests;

namespace YaR.Clouds.Base.Repos.YandexDisk.YadWeb.Requests
{
    class YadAuthPasswordRequest : BaseRequestJson<YadAuthPasswordRequestResult>
    {
        private readonly IAuth _auth;
        private readonly string _csrf;
        private readonly string _trackId;

        public YadAuthPasswordRequest(HttpCommonSettings settings, IAuth auth, string csrf, string trackId) 
            : base(settings, auth)
        {
            _auth = auth;
            _csrf = csrf;
            _trackId = trackId;
        }

        protected override string RelationalUri => "/registration-validations/auth/multi_step/commit_password";

        protected override HttpWebRequest CreateRequest(string baseDomain = null)
        {
            var request = base.CreateRequest("https://passport.yandex.ru");
            return request;
        }

        protected override byte[] CreateHttpContent()
        {
            var keyValues = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("csrf_token", _csrf),
                new KeyValuePair<string, string>("track_id", _trackId),
                new KeyValuePair<string, string>("password", _auth.Password),
                new KeyValuePair<string, string>("retpath", "https://disk.yandex.ru/client/disk")
            };
            var content = new FormUrlEncodedContent(keyValues);
            var d = content.ReadAsByteArrayAsync().Result;
            return d;
        }

        protected override RequestResponse<YadAuthPasswordRequestResult> DeserializeMessage(NameValueCollection responseHeaders, Stream stream)
        {
            var res = base.DeserializeMessage(responseHeaders, stream);

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

        [JsonProperty("retpath")]
        public string RetPath { get; set; }

        [JsonIgnore]
        public string DefaultUid { get; set; }

        [JsonProperty("errors")]
        public List<string> Errors { get; set; }
    }
}