using System.Collections.Generic;
using Newtonsoft.Json;

namespace YaR.Clouds.Base.Repos.YandexDisk.YadWebV2.Models
{
    class YadResourceStatsPostModel : YadPostModel
    {
        private readonly string _prefix;

        public YadResourceStatsPostModel(string path, string prefix = "/disk")
        {
            _prefix = prefix;
            Name = "resourceStats";
            Path = path;
        }

        public string Path { get; set; }

        public override IEnumerable<KeyValuePair<string, string>> ToKvp(int index)
        {
            foreach (var pair in base.ToKvp(index))
                yield return pair;

            yield return new KeyValuePair<string, string>($"path.{index}", WebDavPath.Combine(_prefix, Path));
        }
    }


    class YadResourceStatsRequestData : YadModelDataBase
    {
        [JsonProperty("files_count")]
        public long FilesCount { get; set; }

        [JsonProperty("path")]
        public string Path { get; set; }

        [JsonProperty("size")]
        public long Size { get; set; }
    }

    class YadResourceStatsRequestParams
    {
        [JsonProperty("path")]
        public string Path { get; set; }
    }

}