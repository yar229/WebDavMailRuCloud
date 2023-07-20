using System.Collections.Generic;
using Newtonsoft.Json;

namespace YaR.Clouds.Base.Repos.YandexDisk.YadWeb.Models
{
    class YadGetResourceUrlPostModel : YadPostModel
    {
        public YadGetResourceUrlPostModel(string path)
        {
            Name = "do-get-resource-url";
            Path = path;
        }

        public string Path { get; set; }

        public override IEnumerable<KeyValuePair<string, string>> ToKvp(int index)
        {
            foreach (var pair in base.ToKvp(index))
                yield return pair;

            //yield return new KeyValuePair<string, string>($"id.{index}", WebDavPath.Combine("/disk", Path));
            // 08.07.2023 в браузере при скачивании: "idResource.0"
            yield return new KeyValuePair<string, string>($"idResource.{index}", WebDavPath.Combine("/disk", Path));
        }
    }

    internal class ResourceUrlData : YadModelDataBase
    {
        [JsonProperty("digest")]
        public string Digest { get; set; }

        [JsonProperty("file")]
        public string File { get; set; }
    }

    internal class ResourceUrlParams
    {
        [JsonProperty("id")]
        public string Id { get; set; }
    }
}