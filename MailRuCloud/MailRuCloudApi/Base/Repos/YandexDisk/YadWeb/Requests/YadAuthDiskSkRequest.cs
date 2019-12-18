using System.Net;
using System.Text.RegularExpressions;
using YaR.MailRuCloud.Api.Base.Requests;

namespace YaR.MailRuCloud.Api.Base.Repos.YandexDisk.YadWeb.Requests
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
            request.Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3";
            //request.Headers["Accept-Encoding"] = "gzip, deflate, br";
            request.Headers["Sec-Fetch-Mode"] = "navigate";
            request.Headers["Sec-Fetch-Site"] = "same-site";
            request.Headers["Sec-Fetch-User"] = "?1";
            request.Referer = "https://passport.yandex.ru/auth/list?from=cloud&origin=disk_landing_web_signin_ru&retpath=https%3A%2F%2Fdisk.yandex.ru%2Fclient%2Fdisk&backpath=https%3A%2F%2Fdisk.yandex.ru&mode=edit";
            return request;
        }

        protected override RequestResponse<YadAuthDiskSkRequestResult> DeserializeMessage(string responseText)
        {
            var matchSk = Regex.Match(responseText, @"""sk"":""(?<sk>.+?)""");

            var msg = new RequestResponse<YadAuthDiskSkRequestResult>
            {
                Ok = true,
                Result = new YadAuthDiskSkRequestResult
                {
                    DiskSk = matchSk.Success ? matchSk.Groups["sk"].Value : string.Empty,
                }
            };

            return msg;
        }
    }

    class YadAuthDiskSkRequestResult
    {
        public bool HasError => string.IsNullOrEmpty(DiskSk);

        public string DiskSk { get; set; }
    }

}