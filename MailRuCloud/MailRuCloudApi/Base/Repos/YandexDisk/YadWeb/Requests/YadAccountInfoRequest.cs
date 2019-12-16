using System.Net;
using System.Text;
using Newtonsoft.Json;
using YaR.MailRuCloud.Api.Base.Requests;

namespace YaR.MailRuCloud.Api.Base.Repos.YandexDisk.YadWeb.Requests
{
    class YadAccountInfoRequest : BaseRequestJson<YadRequestResult<DataAccountInfo, ParamsAccountInfo>>
    {
        private readonly YadWebAuth _auth;

        public YadAccountInfoRequest(HttpCommonSettings settings, YadWebAuth auth) : base(settings, auth)
        {
            _auth = auth;
        }

        protected override string RelationalUri => "/models/?_m=space";

        protected override HttpWebRequest CreateRequest(string baseDomain = null)
        {
            var request = base.CreateRequest("https://disk.yandex.ru");
            request.Referer = "https://disk.yandex.ru/client/disk";
            return request;
        }

        protected override byte[] CreateHttpContent()
        {
            var data = Encoding.UTF8.GetBytes($"sk={_auth.DiskSk}&idClient={_auth.Uuid}&_model.0=space");
            return data;
        }
    }

    public class DataAccountInfo
    {
        [JsonProperty("used")]
        public long Used { get; set; }

        [JsonProperty("uid")]
        //[JsonConverter(typeof(Newtonsoft.Json.Converters.ParseStringConverter))]
        public long Uid { get; set; }

        [JsonProperty("filesize_limit")]
        public long FilesizeLimit { get; set; }

        [JsonProperty("free")]
        public long Free { get; set; }

        [JsonProperty("limit")]
        public long Limit { get; set; }

        [JsonProperty("trash")]
        public long Trash { get; set; }

        [JsonProperty("files_count")]
        public long FilesCount { get; set; }
    }

    public class ParamsAccountInfo
    {
        [JsonProperty("locale")]
        public string Locale { get; set; }
    }
}