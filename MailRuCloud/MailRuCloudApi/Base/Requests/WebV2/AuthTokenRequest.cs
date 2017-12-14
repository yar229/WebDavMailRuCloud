using System.Net;
using YaR.MailRuCloud.Api.Base.Requests.Repo;

namespace YaR.MailRuCloud.Api.Base.Requests.WebV2
{
    class AuthTokenRequest : BaseRequestJson<AuthTokenRequest.Result>
    {
        public AuthTokenRequest(HttpCommonSettings settings, IAuth auth) : base(settings, auth)
        {
        }

        protected override string RelationalUri
        {
            get
            {
                const string uri = "/api/v2/tokens/csrf";
                return uri;
            }
        }

        public class Result
        {
            public string email { get; set; }
            public AuthTokenResultBody body { get; set; }
            public long time { get; set; }
            public int status { get; set; }

            public class AuthTokenResultBody
            {
                public string token { get; set; }
            }
        }


    }
}
