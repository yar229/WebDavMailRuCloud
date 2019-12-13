using System.Net;
using System.Net.Mime;
using YaR.MailRuCloud.Api.Base.Requests;

namespace YaR.MailRuCloud.Api.Base.Repos.YadWeb.Requests
{
    class YadDownloadRequest
    {
        public YadDownloadRequest(HttpCommonSettings settings, IAuth authent, string url, long instart, long inend)
        {
            Request = CreateRequest(authent, settings.Proxy, url, instart, inend, settings.UserAgent);
        }

        public HttpWebRequest Request { get; }

        private HttpWebRequest CreateRequest(IAuth authent, IWebProxy proxy, string url, long instart, long inend,  string userAgent)
        {
            //url = "https:" + url;

            var request = (HttpWebRequest) WebRequest.Create(url);

            request.Headers.Add("Accept-Ranges", "bytes");
            request.AddRange(instart, inend);
            request.Proxy = proxy;
            request.CookieContainer = authent.Cookies;
            request.Method = "GET";
            request.ContentType = MediaTypeNames.Application.Octet;
            request.Accept = "*/*";
            request.UserAgent = userAgent;
            request.AllowReadStreamBuffering = false;

            return request;
        }

        public static implicit operator HttpWebRequest(YadDownloadRequest v)
        {
            return v.Request;
        }
    }
}