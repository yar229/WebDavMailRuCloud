using System.Collections.Generic;
using Newtonsoft.Json;

namespace YaR.Clouds.Base.Repos.YandexDisk.YadWeb.Models
{
    class YadCreateFolderPostModel : YadPostModel
    {
        public YadCreateFolderPostModel(string path, bool force = true)
        {
            Name = "do-resource-create-folder";
            Path = path;
            Force = force;
        }

        public string Path { get; set; }
        public bool Force { get; set; }

        public override IEnumerable<KeyValuePair<string, string>> ToKvp(int index)
        {
            foreach (var pair in base.ToKvp(index))
                yield return pair;
            
            yield return new KeyValuePair<string, string>($"id.{index}", WebDavPath.Combine("/disk", Path));
            yield return new KeyValuePair<string, string>($"force.{index}", Force ? "1" : "0");
        }
    }

    class YadCreateFolderRequestData : YadModelDataBase
    {
    }

    class YadCreateFolderRequestParams
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("force")]
        public long Force { get; set; }
    }
}