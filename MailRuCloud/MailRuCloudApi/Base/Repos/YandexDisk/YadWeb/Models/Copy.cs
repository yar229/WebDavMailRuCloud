using System.Collections.Generic;
using Newtonsoft.Json;

namespace YaR.Clouds.Base.Repos.YandexDisk.YadWeb.Models
{
    class YadCopyPostModel : YadPostModel
    {
        public YadCopyPostModel(string sourcePath, string destPath, bool force = true)
        {
            Name = "do-resource-copy";
            Source = sourcePath;
            Destination = destPath;
            Force = force;
        }

        public string Source { get; set; }
        public string Destination { get; set; }
        public bool Force { get; set; }

        public override IEnumerable<KeyValuePair<string, string>> ToKvp(int index)
        {
            foreach (var pair in base.ToKvp(index))
                yield return pair;
            
            yield return new KeyValuePair<string, string>($"src.{index}", WebDavPath.Combine("/disk", Source));
            yield return new KeyValuePair<string, string>($"dst.{index}", WebDavPath.Combine("/disk", Destination));
            yield return new KeyValuePair<string, string>($"force.{index}", Force ? "1" : "0");
        }
    }

    class YadCopyRequestData
    {
        [JsonProperty("at_version")]
        public long AtVersion { get; set; }

        [JsonProperty("oid")]
        public string Oid { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }
    }

    class YadCopyRequestParams
    {
        [JsonProperty("src")]
        public string Src { get; set; }

        [JsonProperty("dst")]
        public string Dst { get; set; }

        [JsonProperty("force")]
        public long Force { get; set; }
    }
}