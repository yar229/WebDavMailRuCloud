using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace YaR.Clouds.Base.Repos.YandexDisk.YadWeb.Models.Media
{
    class YadInitSnapshotPostModel : YadPostModel
    {
        public YadInitSnapshotPostModel()
        {
            Name = "initSnapshot";
        }

        public override IEnumerable<KeyValuePair<string, string>> ToKvp(int index)
        {
            foreach (var pair in base.ToKvp(index))
                yield return pair;
        }
    }

    internal class YadInitSnapshotRequestData : YadModelDataBase
    {
        [JsonProperty("photoslice_id")]
        public string PhotosliceId { get; set; }

        [JsonProperty("href")]
        public string Href { get; set; }

        [JsonProperty("revision")]
        public long Revision { get; set; }

        [JsonProperty("method")]
        public string Method { get; set; }

        [JsonProperty("templated")]
        public bool Templated { get; set; }
    }

    internal class YadInitSnapshotRequestParams
    {
    }
}