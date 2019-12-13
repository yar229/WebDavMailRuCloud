using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using Newtonsoft.Json;
using YaR.MailRuCloud.Api.Base.Requests;

namespace YaR.MailRuCloud.Api.Base.Repos.YadWeb.Requests
{
    class YadAccountInfoRequest : BaseRequestJson<YadAccountInfoRequestResult>
    {
        private readonly HttpCommonSettings _settings;
        private readonly YadWebAuth _auth;

        public YadAccountInfoRequest(HttpCommonSettings settings, YadWebAuth auth) : base(settings, auth)
        {
            _settings = settings;
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


    public partial class YadAccountInfoRequestResult
    {
        public bool HasError => Models?.Any(m => m.Error != null) ?? false;

        [JsonProperty("uid")]
        //[JsonConverter(typeof(ParseStringConverter))]
        public long Uid { get; set; }

        [JsonProperty("login")]
        public string Login { get; set; }

        [JsonProperty("sk")]
        public string Sk { get; set; }

        [JsonProperty("version")]
        public string Version { get; set; }

        [JsonProperty("models")]
        public List<Model> Models { get; set; }
    }

    public partial class Model
    {
        [JsonProperty("model")]
        public string ModelModel { get; set; }

        [JsonProperty("params")]
        public Params Params { get; set; }

        [JsonProperty("data")]
        public Data Data { get; set; }

        [JsonProperty("error")]
        public Error Error { get; set; }
    }

    public partial class Error
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("message")]
        public string Message { get; set; }
    }

    public partial class Data
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

    public class Params
    {
        [JsonProperty("locale")]
        public string Locale { get; set; }
    }
}