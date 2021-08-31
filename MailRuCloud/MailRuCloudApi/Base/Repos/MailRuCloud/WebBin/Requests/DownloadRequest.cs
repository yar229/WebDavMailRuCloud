using System;
using System.Linq;
using System.Net;
using System.Collections.Generic;

using YaR.Clouds.Base.Requests;

namespace YaR.Clouds.Base.Repos.MailRuCloud.WebBin.Requests
{
    class DownloadRequest
    {
        public DownloadRequest(HttpCommonSettings settings, IAuth authent, File file, long instart, long inend, string downServerUrl, IEnumerable<string> publicBaseUrls)
        {
            Request = CreateRequest(settings, authent, file, instart, inend, downServerUrl, publicBaseUrls);
        }

        public HttpWebRequest Request { get; }

        private static HttpWebRequest CreateRequest(HttpCommonSettings settings, IAuth authent, File file, long instart, long inend, string downServerUrl, IEnumerable<string> publicBaseUrls) //(IAuth authent, IWebProxy proxy, string url, long instart, long inend,  string userAgent)
        {
            bool isLinked = file.PublicLinks.Any();

            string url;

            if (isLinked)
            {
                var urii = file.PublicLinks.First().Uri;
                var uriistr = urii.OriginalString;
                var baseura = publicBaseUrls.First(pbu => uriistr.StartsWith(pbu, StringComparison.InvariantCulture));
                if (string.IsNullOrEmpty(baseura))
                    throw new ArgumentException("url does not starts with base url");

                url = $"{downServerUrl}{WebDavPath.EscapeDataString(uriistr.Remove(0, baseura.Length))}";
            }
            else
            {
                url = $"{downServerUrl}{Uri.EscapeDataString(file.FullPath.TrimStart('/'))}";
                url += $"?client_id={settings.ClientId}&token={authent.AccessToken}";
            }

            var uri = new Uri(url);

            HttpWebRequest request = (HttpWebRequest) WebRequest.Create(uri.OriginalString);

            request.AllowAutoRedirect = true;

            request.AddRange(instart, inend);
            request.Proxy = settings.Proxy;
            //request.CookieContainer = authent.Cookies;
            request.Method = "GET";
            //request.Accept = "*/*";
            //request.UserAgent = settings.UserAgent;
            //request.Host = uri.Host;
            request.AllowWriteStreamBuffering = false;

            if (isLinked)
                request.Headers.Add("Accept-Ranges", "bytes");

            request.Timeout = 15 * 1000;
            request.ReadWriteTimeout = 15 * 1000;

            return request;
        }

        public static implicit operator HttpWebRequest(DownloadRequest v)
        {
            return v.Request;
        }
    }
}
