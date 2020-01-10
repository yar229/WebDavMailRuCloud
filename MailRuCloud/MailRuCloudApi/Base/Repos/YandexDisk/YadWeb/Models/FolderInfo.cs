using System.Collections.Generic;
using Newtonsoft.Json;

namespace YaR.Clouds.Base.Repos.YandexDisk.YadWeb.Models
{
    class YadFolderInfoPostModel : YadPostModel
    {
        private readonly string _pathPrefix;

        public YadFolderInfoPostModel(string path, string pathPrefix = "/disk")
        {
            _pathPrefix = pathPrefix;
            Name = "resources";
            Path = path;
        }

        public string Path { get; set; }
        public int Order { get; set; } = 1;
        public string SortBy { get; set; } = "name";
        public int Offset { get; set; } = 0;
        public int Amount { get; set; } = int.MaxValue;

        public override IEnumerable<KeyValuePair<string, string>> ToKvp(int index)
        {
            foreach (var pair in base.ToKvp(index))
                yield return pair;
            
            yield return new KeyValuePair<string, string>($"idContext.{index}", WebDavPath.Combine(_pathPrefix, Path));
            yield return new KeyValuePair<string, string>($"order.{index}", Order.ToString());
            yield return new KeyValuePair<string, string>($"sort.{index}", SortBy);
            yield return new KeyValuePair<string, string>($"offset.{index}", Offset.ToString());
            yield return new KeyValuePair<string, string>($"amount.{index}", Amount.ToString());
        }
    }

    internal class YadFolderInfoRequestData : YadModelDataBase
    {
        [JsonProperty("resources")]
        public List<FolderInfoDataResource> Resources { get; set; }
    }

    internal class FolderInfoDataResource
    {
        [JsonProperty("ctime")]
        public long Ctime { get; set; }

        [JsonProperty("meta")]
        public Meta Meta { get; set; }

        [JsonProperty("mtime")]
        public long Mtime { get; set; }

        [JsonProperty("path")]
        public string Path { get; set; }

        [JsonProperty("utime")]
        public long Utime { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("etime", NullValueHandling = NullValueHandling.Ignore)]
        public long? Etime { get; set; }
    }

    class Meta
    {
        [JsonProperty("file_id")]
        public string FileId { get; set; }

        [JsonProperty("resource_id")]
        public string ResourceId { get; set; }

        [JsonProperty("mimetype", NullValueHandling = NullValueHandling.Ignore)]
        public string Mimetype { get; set; }

        [JsonProperty("drweb", NullValueHandling = NullValueHandling.Ignore)]
        public long? Drweb { get; set; }

        [JsonProperty("sizes", NullValueHandling = NullValueHandling.Ignore)]
        public List<Size> Sizes { get; set; }

        [JsonProperty("mediatype", NullValueHandling = NullValueHandling.Ignore)]
        public string Mediatype { get; set; }

        [JsonProperty("etime", NullValueHandling = NullValueHandling.Ignore)]
        public long? Etime { get; set; }

        [JsonProperty("versioning_status", NullValueHandling = NullValueHandling.Ignore)]
        public string VersioningStatus { get; set; }

        [JsonProperty("size", NullValueHandling = NullValueHandling.Ignore)]
        public long? Size { get; set; }

        [JsonProperty("video_info", NullValueHandling = NullValueHandling.Ignore)]
        public VideoInfo VideoInfo { get; set; }

        [JsonProperty("short_url")]
        public string UrlShort { get; set; }
    }

    class Size
    {
        [JsonProperty("url")]
        public string Url { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }
    }

    class VideoInfo
    {
        [JsonProperty("format")]
        public string Format { get; set; }

        [JsonProperty("creationTime")]
        public long CreationTime { get; set; }

        [JsonProperty("streams")]
        public List<Stream> Streams { get; set; }

        [JsonProperty("startTime")]
        public long StartTime { get; set; }

        [JsonProperty("duration")]
        public long Duration { get; set; }

        [JsonProperty("bitRate")]
        public long BitRate { get; set; }
    }

    class Stream
    {
        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("frameRate", NullValueHandling = NullValueHandling.Ignore)]
        public long? FrameRate { get; set; }

        [JsonProperty("displayAspectRatio", NullValueHandling = NullValueHandling.Ignore)]
        public DisplayAspectRatio DisplayAspectRatio { get; set; }

        [JsonProperty("codec")]
        public string Codec { get; set; }

        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("bitRate")]
        public long BitRate { get; set; }

        [JsonProperty("dimension", NullValueHandling = NullValueHandling.Ignore)]
        public Dimension Dimension { get; set; }

        [JsonProperty("channelsCount", NullValueHandling = NullValueHandling.Ignore)]
        public long? ChannelsCount { get; set; }

        [JsonProperty("stereo", NullValueHandling = NullValueHandling.Ignore)]
        public bool? Stereo { get; set; }

        [JsonProperty("sampleFrequency", NullValueHandling = NullValueHandling.Ignore)]
        public long? SampleFrequency { get; set; }
    }

    class Dimension
    {
        [JsonProperty("width")]
        public long Width { get; set; }

        [JsonProperty("height")]
        public long Height { get; set; }
    }

    class DisplayAspectRatio
    {
        [JsonProperty("denom")]
        public long Denom { get; set; }

        [JsonProperty("num")]
        public long Num { get; set; }
    }

    internal class YadFolderInfoRequestParams
    {
        [JsonProperty("idContext")]
        public string IdContext { get; set; }

        [JsonProperty("order")]
        public long Order { get; set; }

        [JsonProperty("sort")]
        public string Sort { get; set; }

        [JsonProperty("offset")]
        public long Offset { get; set; }

        [JsonProperty("amount")]
        public long Amount { get; set; }
    }
}