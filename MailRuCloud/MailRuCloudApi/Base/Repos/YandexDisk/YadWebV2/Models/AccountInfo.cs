using Newtonsoft.Json;

namespace YaR.Clouds.Base.Repos.YandexDisk.YadWebV2.Models
{
    class YadAccountInfoPostModel : YadPostModel
    {
        public YadAccountInfoPostModel()
        {
            Name = "space";
        }
    }

    public class YadAccountInfoRequestData : YadModelDataBase
    {
        [JsonProperty("used")]
        public long Used { get; set; }

        [JsonProperty("uid")]
        public long Uid { get; set; }

        [JsonProperty("filesize_limit")]
        public long FilesizeLimit { get; set; }

        [JsonProperty("free")]
        public long Free { get; set; }

        [JsonProperty("limit")]
        public long Limit { get; set; }

        [JsonProperty("trash")]
        public long Trash { get; set; }

        [JsonProperty("files_count")]
        public long FilesCount { get; set; }
    }

    public class YadAccountInfoRequestParams
    {
        [JsonProperty("locale")]
        public string Locale { get; set; }
    }
}