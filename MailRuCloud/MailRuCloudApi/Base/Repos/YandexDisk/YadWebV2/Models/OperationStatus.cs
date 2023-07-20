using System.Collections.Generic;
using Newtonsoft.Json;

namespace YaR.Clouds.Base.Repos.YandexDisk.YadWebV2.Models
{
    internal class YadOperationStatusPostModel : YadPostModel
    {
        public YadOperationStatusPostModel(string oid)
        {
            Name = "do-status-operation";
            Oid = oid;
        }

        public string Oid { get; set; }

        public override IEnumerable<KeyValuePair<string, string>> ToKvp(int index)
        {
            foreach (var pair in base.ToKvp(index))
                yield return pair;
            
            yield return new KeyValuePair<string, string>($"oid.{index}", Oid);
        }
    }


    internal class YadOperationStatusData : YadModelDataBase
    {
        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("state")]
        public string State { get; set; }

        [JsonProperty("at_version")]
        public long AtVersion { get; set; }
    }

    internal class YadOperationStatusParams
    {
        [JsonProperty("oid")]
        public string Oid { get; set; }
    }
}
