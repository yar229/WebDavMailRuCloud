using Newtonsoft.Json;

namespace YaR.Clouds.Base.Repos.YandexDisk.YadWebV2.Models.Media
{
    class YadGetAlbumsSlicesPostModel : YadPostModel
    {
        public YadGetAlbumsSlicesPostModel()
        {
            Name = "getAlbumsSlices";
        }

        //public override IEnumerable<KeyValuePair<string, string>> ToKvp(int index)
        //{
        //    foreach (var pair in base.ToKvp(index))
        //        yield return pair;
        //}
    }

    internal class YadGetAlbumsSlicesRequestParams
    {
    }


    internal class YadGetAlbumsSlicesRequestData : YadModelDataBase
    {
        [JsonProperty("albums")]
        public GAlbums Albums { get; set; }
    }

    internal class GAlbums
    {
        [JsonProperty("videos")]
        public GAlbum Videos { get; set; }

        [JsonProperty("photounlim")]
        public GAlbum Photounlim { get; set; }

        [JsonProperty("camera")]
        public GAlbum Camera { get; set; }
    }

    internal class GAlbum
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("count")]
        public long Count { get; set; }

        [JsonProperty("preview")]
        public string Preview { get; set; }
    }
}