using System.Collections.Generic;
using Newtonsoft.Json;
using YaR.MailRuCloud.Api.Base.Requests;

namespace YaR.MailRuCloud.Api.Base.Repos.YandexDisk.YadWeb.Requests
{
    class YadItemInfoRequest : YadBaseRequestJson<YadRequestResult<YadItemInfoRequestData, YadItemInfoRequestParams>>
    {
        private readonly string _path;

        public YadItemInfoRequest(HttpCommonSettings settings, YadWebAuth auth, string path)  : base(settings, auth)
        {
            _path = path;
        }

        protected override string RelationalUri => "/models/?_m=resource";

        protected override IEnumerable<YadPostModel> CreateModels()
        {
            var pd = new YadItemInfoPostModel
            {
                Path = _path,
            };

            yield return pd;
        }
    }


    class YadItemInfoPostModel : YadPostModel
    {
        public YadItemInfoPostModel()
        {
            Name = "resource";
        }

        public string Path { get; set; }

        public override IEnumerable<KeyValuePair<string, string>> ToKvp(int index)
        {
            foreach (var pair in base.ToKvp(index))
                yield return pair;
            
            yield return new KeyValuePair<string, string>($"id.{index}", WebDavPath.Combine("/disk/", Path));
        }
    }


    class YadItemInfoRequestData
    {
        [JsonProperty("ctime")]
        public long Ctime { get; set; }

        [JsonProperty("meta")]
        public YadItemInfoRequestMeta Meta { get; set; }

        [JsonProperty("mtime")]
        public long Mtime { get; set; }

        [JsonProperty("path")]
        public string Path { get; set; }

        [JsonProperty("utime")]
        public long Utime { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }
    }

    class YadItemInfoRequestMeta
    {
        [JsonProperty("mimetype")]
        public string Mimetype { get; set; }

        [JsonProperty("drweb")]
        public long Drweb { get; set; }

        [JsonProperty("resource_id")]
        public string ResourceId { get; set; }

        [JsonProperty("mediatype")]
        public string Mediatype { get; set; }

        [JsonProperty("file_id")]
        public string FileId { get; set; }

        [JsonProperty("versioning_status")]
        public string VersioningStatus { get; set; }

        [JsonProperty("size")]
        public long Size { get; set; }
    }

    class YadItemInfoRequestParams
    {
        [JsonProperty("id")]
        public string Id { get; set; }
    }
}