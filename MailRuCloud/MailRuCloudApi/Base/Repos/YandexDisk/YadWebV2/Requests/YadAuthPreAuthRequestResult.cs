using System.Collections.Specialized;
using System.Net;
using System.Text.RegularExpressions;
using YaR.Clouds.Base.Requests;

namespace YaR.Clouds.Base.Repos.YandexDisk.YadWebV2.Requests
{
    class YadPreAuthRequest : BaseRequestString<YadAuthPreAuthRequestResult>
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

        protected override RequestResponse<YadAuthPreAuthRequestResult> DeserializeMessage(NameValueCollection responseHeaders, string responseText)
        {
            var matchCsrf = Regex.Match(responseText, @"""csrf"":""(?<csrf>.*?)""");
            var matchUuid = Regex.Match(responseText, @"""process_uuid"":""(?<uuid>.*?)""");  //var matchUuid = Regex.Match(responseText, @"process_uuid(?<uuid>\S+?)&quot;");

            var msg = new RequestResponse<YadAuthPreAuthRequestResult>
            {
                Ok = matchCsrf.Success && matchUuid.Success,
                Result = new YadAuthPreAuthRequestResult
                {
                    Csrf = matchCsrf.Success ? matchCsrf.Groups["csrf"].Value : string.Empty,
                    ProcessUUID = matchUuid.Success ? matchUuid.Groups["uuid"].Value : string.Empty
                }
            };

            return msg;
        }
    }

    class YadAuthPreAuthRequestResult
    {
        public string Csrf { get; set; }
        public string ProcessUUID { get; set; }
    }
}
