using System;
using System.Net;
using System.Text;
using Newtonsoft.Json;
using YaR.MailRuCloud.Api.Base.Requests;

namespace YaR.MailRuCloud.Api.Base.Repos.YadWeb.Requests
{
    class YadGetResourceUrlRequest : BaseRequestJson<YadRequestResult<ResourceUrlData, ResourceUrlParams>>
    {
        private readonly YadWebAuth _auth;
        private readonly string _path;

        public YadGetResourceUrlRequest(HttpCommonSettings settings, YadWebAuth auth, string path)  : base(settings, auth)
        {
            _auth = auth;
            _path = path;
        }

        protected override string RelationalUri => "/models/?_m=do-get-resource-url";

        protected override HttpWebRequest CreateRequest(string baseDomain = null)
        {
            var request = base.CreateRequest("https://disk.yandex.ru");
            request.Referer = "https://disk.yandex.ru/client/disk";
            return request;
        }

        protected override byte[] CreateHttpContent()
        {
            var data = Encoding.UTF8.GetBytes($"sk={_auth.DiskSk}&idClient={_auth.Uuid}&_model.0=do-get-resource-url" +
                                              $"&id.0={WebDavPath.Combine("/disk", _path)}");
            return data;
        }
    }

    public class ResourceUrlData
    {
        [JsonProperty("digest")]
        public string Digest { get; set; }

        [JsonProperty("file")]
        public string File { get; set; }
    }

    public partial class ResourceUrlParams
    {
        [JsonProperty("id")]
        public string Id { get; set; }
    }
}