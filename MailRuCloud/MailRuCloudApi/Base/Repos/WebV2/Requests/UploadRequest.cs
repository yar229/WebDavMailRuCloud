using System;
using System.Net;
using YaR.MailRuCloud.Api.Base.Requests;

namespace YaR.MailRuCloud.Api.Base.Repos.WebV2.Requests
{
    class UploadRequest
    {
        public UploadRequest(string shardUrl, File file, IAuth authent, HttpCommonSettings settings)
        {
            Request = CreateRequest(shardUrl, authent, settings.Proxy, file, settings.UserAgent);
        }

        public HttpWebRequest Request { get; }

        private HttpWebRequest CreateRequest(string shardUrl, IAuth authent, IWebProxy proxy, File file, string userAgent)
        {
            var url = new Uri($"{shardUrl}?cloud_domain=2&{authent.Login}");

            var request = (HttpWebRequest)WebRequest.Create(url.OriginalString);
            request.Proxy = proxy;
            request.CookieContainer = authent.Cookies;
            request.Method = "PUT";
            request.ContentLength = file.OriginalSize; // + boundary.Start.LongLength + boundary.End.LongLength;
            request.Referer = $"{ConstSettings.CloudDomain}/home/{Uri.EscapeDataString(file.Path)}";
            request.Headers.Add("Origin", ConstSettings.CloudDomain);
            request.Host = url.Host;
            //request.ContentType = $"multipart/form-data; boundary=----{boundary.Guid}";
            request.Accept = "*/*";
            request.UserAgent = userAgent;
            request.AllowWriteStreamBuffering = false;
            return request;
        }

        public static implicit operator HttpWebRequest(UploadRequest v)
        {
            return v.Request;
        }
    }
}