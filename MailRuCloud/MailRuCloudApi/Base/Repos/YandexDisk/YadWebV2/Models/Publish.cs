using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace YaR.Clouds.Base.Repos.YandexDisk.YadWebV2.Models
{
    class YadPublishPostModel : YadPostModel
    {
        public YadPublishPostModel(string path, bool reverse)
        {
            Name = "do-resource-publish";
            Path = path;
            Reverse = reverse;
        }

        public string Path { get; set; }
        public bool Reverse { get; set; }

        public override IEnumerable<KeyValuePair<string, string>> ToKvp(int index)
        {
            foreach (var pair in base.ToKvp(index))
                yield return pair;
            
            yield return new KeyValuePair<string, string>($"id.{index}", WebDavPath.Combine("/disk/", Path));
            yield return new KeyValuePair<string, string>($"reverse.{index}", Reverse ? "true" : "false");
        }
    }

    public class YadPublishRequestData : YadModelDataBase
    {
        [JsonProperty("url")]
        public string Url { get; set; }

        [JsonProperty("short_url")]
        public string ShortUrl { get; set; }

        [JsonProperty("hash")]
        public string Hash { get; set; }

        [JsonProperty("short_url_named")]
        public Uri ShortUrlNamed { get; set; }
    }

    public class YadPublishRequestParams
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("reverse")]
        public bool Reverse { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }
    }
}