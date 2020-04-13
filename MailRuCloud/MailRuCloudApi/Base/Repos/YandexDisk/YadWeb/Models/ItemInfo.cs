using System.Collections.Generic;
using Newtonsoft.Json;

namespace YaR.Clouds.Base.Repos.YandexDisk.YadWeb.Models
{
    class YadItemInfoPostModel : YadPostModel
    {
        private readonly string _prefix;

        public YadItemInfoPostModel(string path, string prefix = "/disk")
        {
            _prefix = prefix;
            Name = "resource";
            Path = path;
        }

        public string Path { get; set; }

        public override IEnumerable<KeyValuePair<string, string>> ToKvp(int index)
        {
            foreach (var pair in base.ToKvp(index))
                yield return pair;

            yield return new KeyValuePair<string, string>($"id.{index}", WebDavPath.Combine(_prefix, Path));

            //if (Path == "/Camera")
            //{
            //    yield return new KeyValuePair<string, string>($"id.{index}", "/photounlim/");
            //}
            //else
            //{
            //    yield return new KeyValuePair<string, string>($"id.{index}", WebDavPath.Combine("/disk/", Path));
            //}
        }
    }

    class YadItemInfoRequestData : YadModelDataBase
    {
        [JsonProperty("ctime")]
        public long Ctime { get; set; }

        [JsonProperty("meta")]
        public YadItemInfoRequestMeta Meta { get; set; }

        [JsonProperty("mtime")]
        public ulong Mtime { get; set; }

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

        [JsonProperty("short_url")]
        public string UrlShort { get; set; }
    }

    class YadItemInfoRequestParams
    {
        [JsonProperty("id")]
        public string Id { get; set; }
    }
}