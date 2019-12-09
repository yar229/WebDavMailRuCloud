using System.Net;
using System.Text.RegularExpressions;
using YaR.MailRuCloud.Api.Base.Requests;

namespace YaR.MailRuCloud.Api.Base.Repos.YadWeb.Requests
{
    class YadPreAuthRequestResult
    {
        public string Csrf { get; set; }
        public string ProcessUUID { get; set; }
    }

    class YadPreAuthRequest : BaseRequestString<YadPreAuthRequestResult>
    {
        public YadPreAuthRequest(HttpCommonSettings settings, IAuth auth) 
            : base(settings, auth)
        {
        }

        protected override HttpWebRequest CreateRequest(string baseDomain = null)
        {
            var request = base.CreateRequest("https://passport.yandex.ru");
            return request;
        }

        protected override string RelationalUri => "/auth";

        //protected override byte[] CreateHttpContent()
        //{
        //    string data = $"Login={Uri.EscapeUriString(Auth.Login)}&Domain={CommonSettings.Domain}&Password={Uri.EscapeUriString(Auth.Password)}";

        //    return Encoding.UTF8.GetBytes(data);
        //}

        protected override RequestResponse<YadPreAuthRequestResult> DeserializeMessage(string responseText)
        {
            var matchCsrf = Regex.Match(responseText, @"data-csrf=""(<csrf>.*?)""");
            var matchUuid = Regex.Match(responseText, @"process_uuid=(<uuid>.*?)""");

            var msg = new RequestResponse<YadPreAuthRequestResult>
            {
                Ok = true,
                Result = new YadPreAuthRequestResult
                {
                    Csrf = matchCsrf.Success ? matchCsrf.Groups["csrf"].Value : string.Empty,
                    ProcessUUID = matchUuid.Success ? matchUuid.Groups["uuid"].Value : string.Empty,
                }
            };

            return msg;
        }
    }
}
