using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using Newtonsoft.Json;
using YaR.MailRuCloud.Api.Base.Requests;

namespace YaR.MailRuCloud.Api.Base.Repos.YadWeb.Requests
{
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
            return request;
        }

        protected override byte[] CreateHttpContent()
        {
            
            var keyValues = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("csrf_token", _csrf),
                new KeyValuePair<string, string>("process_uuid", _uuid),
                new KeyValuePair<string, string>("login", _auth.Login),
            };
            FormUrlEncodedContent z = new FormUrlEncodedContent(keyValues);
            var d = z.ReadAsByteArrayAsync().Result;
            return d;

            //var data = Encoding.UTF8.GetBytes($"csrf_token={_csrf}&process_uuid={_uuid}&login={_auth.Login}");
            //return data;
        }

    }
}