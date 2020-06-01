using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mime;
using YaR.Clouds.Base.Requests;
using YaR.Clouds.Base.Requests.Types;
using YaR.Clouds.Common;

namespace YaR.Clouds.Base.Repos.MailRuCloud.WebV2.Requests
{
    class DownloadRequest
    {
        public DownloadRequest(File file, long instart, long inend, IAuth authent, HttpCommonSettings settings, Cached<Dictionary<ShardType, ShardInfo>> shards)
        {
            Request = CreateRequest(authent, settings.Proxy, file, instart, inend, settings.UserAgent, shards);
        }

        public HttpWebRequest Request { get; }

        private HttpWebRequest CreateRequest(IAuth authent, IWebProxy proxy, File file, long instart, long inend,  string userAgent, Cached<Dictionary<ShardType, ShardInfo>> shards)
        {
            bool isLinked = file.PublicLinks.Any();

            string downloadkey = isLinked
                ? authent.DownloadToken
                : authent.AccessToken;

            var shard = isLinked
                ? shards.Value[ShardType.WeblinkGet]
                : shards.Value[ShardType.Get];

            string url = !isLinked
                ? $"{shard.Url}{Uri.EscapeDataString(file.FullPath)}"
                : $"{shard.Url}{file.PublicLinks.First().Uri.PathAndQuery.Remove(0, "/public".Length)}?key={downloadkey}";

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

        public static implicit operator HttpWebRequest(DownloadRequest v)
        {
            return v.Request;
        }
    }
}