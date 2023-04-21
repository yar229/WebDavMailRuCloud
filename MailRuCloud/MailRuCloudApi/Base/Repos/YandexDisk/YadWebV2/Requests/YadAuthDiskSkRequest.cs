using System.Collections.Specialized;
using System.Net;
using System.Text.RegularExpressions;
using YaR.Clouds.Base.Requests;

namespace YaR.Clouds.Base.Repos.YandexDisk.YadWebV2.Requests
{
    class YadAuthDiskSkRequest : BaseRequestString<YadAuthDiskSkRequestResult>
    {
        public YadAuthDiskSkRequest(HttpCommonSettings settings, YadWebAuth auth) : base(settings, auth)
        {
        }

        protected override string RelationalUri => "/client/disk";

        protected override HttpWebRequest CreateRequest(string baseDomain = null)
        {
            var request = base.CreateRequest("https://disk.yandex.ru");
            request.Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,image/avif,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3;q=0.9";
            request.Headers["Accept-Encoding"] = "gzip, deflate, br";
            request.Headers["Accept-Language"] = "ru-RU,ru;q=0.9,en-US;q=0.8,en;q=0.7,es;q=0.6";
            request.Headers["sec-ch-ua"] = "\" Not A; Brand\";v=\"99\", \"Chromium\";v=\"99\", \"Google Chrome\";v=\"99\"";
            request.Headers["sec-ch-ua-mobile"] = "?0";
            request.Headers["sec-ch-ua-platform"] = "\"Windows\"";
            request.Headers["Sec-Fetch-Dest"] = "document";
            request.Headers["Sec-Fetch-Mode"] = "navigate";
            request.Headers["Sec-Fetch-Site"] = "same-site";
            request.Headers["Sec-Fetch-User"] = "?1";
            request.Headers["Upgrade-Insecure-Requests"] = "1";

            return request;
        }

        protected override RequestResponse<YadAuthDiskSkRequestResult> DeserializeMessage(NameValueCollection responseHeaders, string responseText)
        {
            var matchSk = Regex.Match(responseText, Regex1);

            var msg = new RequestResponse<YadAuthDiskSkRequestResult>
            {
                Ok = true,
                Result = new YadAuthDiskSkRequestResult
                {
                    DiskSk = matchSk.Success ? matchSk.Groups["sk"].Value : string.Empty,
                    HtmlResponse = responseText
                }
            };

            return msg;
        }

        private const string Regex1 = @"""sk"":""(?<sk>.+?)""";
        //private const string _regex2 = @"sk=(?<sk>.*?)&";  // не надо, значит, уже все плохо
    }

    class YadAuthDiskSkRequestResult
    {
        public bool HasError => string.IsNullOrEmpty(DiskSk);

        public string DiskSk { get; set; }

        public string HtmlResponse { get; set; }
    }

}