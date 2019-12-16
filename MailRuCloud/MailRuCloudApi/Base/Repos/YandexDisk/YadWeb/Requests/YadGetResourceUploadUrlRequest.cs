using System.Collections.Generic;
using System.Net;
using Newtonsoft.Json;
using YaR.MailRuCloud.Api.Base.Requests;

namespace YaR.MailRuCloud.Api.Base.Repos.YandexDisk.YadWeb.Requests
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
            request.Referer = "https://disk.yandex.ru/client/disk";;
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


    class YadGetResourceUploadUrlRequest : YadBaseRequestJson<YadRequestResult<ResourceUploadUrlData, ResourceUploadUrlParams>>
    {
        private readonly string _path;
        private readonly long _size;
        private readonly bool _force;

        public YadGetResourceUploadUrlRequest(HttpCommonSettings settings, YadWebAuth auth, string path, long size, bool force = false)  : base(settings, auth)
        {
            _path = path;
            _size = size;
            _force = force;
        }

        protected override string RelationalUri => "/models/?_m=do-resource-upload-url";

        protected override IEnumerable<YadPostModel> CreateModels()
        {
            yield return new YadGetResourceUploadUrlPostModel
            {
                Destination = WebDavPath.Combine("/disk", _path),
                Force = _force,
                Size = _size
            };
        }
    }

    class YadGetResourceUploadUrlPostModel : YadPostModel
    {
        public YadGetResourceUploadUrlPostModel()
        {
            Name = "do-resource-upload-url";
        }

        public string Destination { get; set; }
        public bool Force { get; set; }
        public long Size { get; set; }
        public string Md5 { get; set; }
        public string Sha256 { get; set; }

        public override IEnumerable<KeyValuePair<string, string>> ToKvp(int index)
        {
            foreach (var pair in base.ToKvp(index))
                yield return pair;
            
            yield return new KeyValuePair<string, string>($"dst.{index}", Destination);
            yield return new KeyValuePair<string, string>($"force.{index}", Force ? "1" : "0");
            yield return new KeyValuePair<string, string>($"size.{index}", Size.ToString());
            yield return new KeyValuePair<string, string>($"md5.{index}", Md5);
            yield return new KeyValuePair<string, string>($"sha256.{index}", Sha256);
        }
    }

    internal class ResourceUploadUrlData
    {
        [JsonProperty("at_version")]
        public long AtVersion { get; set; }

        [JsonProperty("upload_url")]
        public string UploadUrl { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("oid")]
        public string Oid { get; set; }
    }

    internal class ResourceUploadUrlParams
    {
        [JsonProperty("dst")]
        public string Dst { get; set; }

        [JsonProperty("force")]
        public long Force { get; set; }

        [JsonProperty("size")]
        public long Size { get; set; }

        [JsonProperty("md5")]
        public string Md5 { get; set; }

        [JsonProperty("sha256")]
        public string Sha256 { get; set; }
    }


}