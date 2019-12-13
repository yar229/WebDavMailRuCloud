using System.Collections.Generic;
using System.Net;
using System.Text;
using Newtonsoft.Json;
using YaR.MailRuCloud.Api.Base.Requests;

namespace YaR.MailRuCloud.Api.Base.Repos.YadWeb.Requests
{
    class YadFolderInfoRequest : BaseRequestJson<YadRequestResult<DataResources, ParamsResources>>
    {
        private readonly YadWebAuth _auth;
        private readonly string _path;
        private readonly int _offset;
        private readonly int _limit;


        public YadFolderInfoRequest(HttpCommonSettings settings, YadWebAuth auth, string path, int offset = 0, int limit = int.MaxValue)  : base(settings, auth)
        {
            _auth = auth;
            _path = path;
            _offset = offset;
            _limit = limit;
        }

        protected override string RelationalUri => "/models/?_m=space";

        protected override HttpWebRequest CreateRequest(string baseDomain = null)
        {
            var request = base.CreateRequest("https://disk.yandex.ru");
            request.Referer = "https://disk.yandex.ru/client/disk";
            return request;
        }

        protected override byte[] CreateHttpContent()
        {
            var data = Encoding.UTF8.GetBytes($"sk={_auth.DiskSk}&idClient={_auth.Uuid}&_model.0=resources" +
                                              $"&idContext.0={WebDavPath.Combine("/disk", _path)}" +
                                              $"&order.0=1" +
                                              $"&sort.0=name" +
                                              $"&offset.0={_offset}" +
                                              $"&amount.0={_limit}");
            return data;
        }
    }



    public class DataResources
    {
        [JsonProperty("resources")]
        public List<Resource> Resources { get; set; }
    }

    public class Resource
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

    public class Meta
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
    }

    public class Size
    {
        [JsonProperty("url")]
        public string Url { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }
    }

    public class VideoInfo
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

    public class Stream
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
        //[System.Text.Json.Serialization.JsonConverter(typeof(ParseStringConverter))]
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

    public class Dimension
    {
        [JsonProperty("width")]
        public long Width { get; set; }

        [JsonProperty("height")]
        public long Height { get; set; }
    }

    public class DisplayAspectRatio
    {
        [JsonProperty("denom")]
        public long Denom { get; set; }

        [JsonProperty("num")]
        public long Num { get; set; }
    }

    public class ParamsResources
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