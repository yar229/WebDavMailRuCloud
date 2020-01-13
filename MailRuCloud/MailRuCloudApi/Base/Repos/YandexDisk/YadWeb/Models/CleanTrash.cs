using Newtonsoft.Json;

namespace YaR.Clouds.Base.Repos.YandexDisk.YadWeb.Models
{
    class YadCleanTrashPostModel : YadPostModel
    {
        public YadCleanTrashPostModel()
        {
            Name = "do-clean-trash";
        }
    }

    internal class YadCleanTrashData : YadModelDataBase
    {
        [JsonProperty("at_version")]
        public long AtVersion { get; set; }

        [JsonProperty("oid")]
        public string Oid { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }
    }

    internal class YadCleanTrashParams
    {
    }
}
