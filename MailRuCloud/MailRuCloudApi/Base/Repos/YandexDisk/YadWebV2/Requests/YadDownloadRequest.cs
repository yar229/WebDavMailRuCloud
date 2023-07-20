using System.Net;
using System.Net.Mime;
using YaR.Clouds.Base.Requests;

namespace YaR.Clouds.Base.Repos.YandexDisk.YadWebV2.Requests
{
    class YadDownloadRequest
    {
        public YadDownloadRequest(HttpCommonSettings settings, IAuth authent, string url, long instart, long inend)
        {
            Request = CreateRequest(authent, settings.Proxy, url, instart, inend, settings.UserAgent);
        }

        public HttpWebRequest Request { get; }

        private static HttpWebRequest CreateRequest(IAuth authent, IWebProxy proxy, string url, long instart, long inend,  string userAgent)
        {
            var request = (HttpWebRequest) WebRequest.Create(url);

            request.Headers.Add("Accept-Ranges", "bytes");
            request.Headers.Add("Upgrade-Insecure-Requests", "1");
            request.Headers.Add("Sec-Fetch-Site", "cross-site");
            request.Headers.Add("Sec-Fetch-Mode", "nested-navigate");

            request.AddRange(instart, inend);
            request.Proxy = proxy;
            request.CookieContainer = authent.Cookies;
            request.Method = "GET";
            request.ContentType = MediaTypeNames.Application.Octet;
            request.Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3";
            request.UserAgent = userAgent;
            request.AllowReadStreamBuffering = false;
            request.AllowAutoRedirect = true;
            request.Referer = "https://disk.yandex.ru/client/disk";

            return request;
        }

        public static implicit operator HttpWebRequest(YadDownloadRequest v)
        {
            return v.Request;
        }
    }
}