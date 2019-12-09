using System.Text;
using Newtonsoft.Json;
using YaR.MailRuCloud.Api.Base.Requests;

namespace YaR.MailRuCloud.Api.Base.Repos.YadWeb.Requests
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

        protected override byte[] CreateHttpContent()
        {
            var data = Encoding.UTF8.GetBytes($"csrf_token={_csrf}&track_id={_trackId}&password={_auth.Password}");
            return data;
        }
    }

    class YadAuthPasswordRequestResult
    {
        public bool HasError => Status == "error";

        [JsonProperty("status")]
        public string Status { get; set; }
    }
}