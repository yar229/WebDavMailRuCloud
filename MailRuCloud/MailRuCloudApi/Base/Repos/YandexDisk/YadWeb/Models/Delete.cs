using System.Collections.Generic;
using Newtonsoft.Json;

namespace YaR.Clouds.Base.Repos.YandexDisk.YadWeb.Models
{
    class YadDeletePostModel : YadPostModel
    {
        public YadDeletePostModel(string path)
        {
            Name = "do-resource-delete";
            Path = path;
        }

        public string Path { get; set; }

        public override IEnumerable<KeyValuePair<string, string>> ToKvp(int index)
        {
            foreach (var pair in base.ToKvp(index))
                yield return pair;
            
            yield return new KeyValuePair<string, string>($"id.{index}", WebDavPath.Combine("/disk", Path));
        }
    }

    public class YadDeleteRequestData : YadModelDataBase
    {
        [JsonProperty("at_version")]
        public long AtVersion { get; set; }

        [JsonProperty("oid")]
        public string Oid { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }
    }

    public class YadDeleteRequestParams
    {
        [JsonProperty("id")]
        public string Id { get; set; }
    }
}