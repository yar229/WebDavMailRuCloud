using System.Net;
using YaR.Clouds.Base.Repos.MailRuCloud;
using YaR.Clouds.Base.Requests;

namespace YaR.Clouds.Base.Repos.YandexDisk.YadWebV2.Requests
{
    class YadUploadRequest
    {
        public YadUploadRequest(HttpCommonSettings settings, YadWebAuth authent, string url, long size)
        {
            Request = CreateRequest(url, authent, settings.Proxy, size, settings.UserAgent);
        }

        public HttpWebRequest Request { get; }

        private HttpWebRequest CreateRequest(string url, YadWebAuth authent, IWebProxy proxy, long size, string userAgent)
        {
            var request = (HttpWebRequest)WebRequest.Create(url);
            request.Proxy = proxy;
            request.CookieContainer = authent.Cookies;
            request.Method = "PUT";
            request.ContentLength = size;
            request.Referer = "https://disk.yandex.ru/client/disk";
            request.Headers.Add("Origin", ConstSettings.CloudDomain);
            request.Accept = "*/*";
            request.UserAgent = userAgent;
            request.AllowWriteStreamBuffering = false;
            return request;
        }

        public static implicit operator HttpWebRequest(YadUploadRequest v)
        {
            return v.Request;
        }
    }
}