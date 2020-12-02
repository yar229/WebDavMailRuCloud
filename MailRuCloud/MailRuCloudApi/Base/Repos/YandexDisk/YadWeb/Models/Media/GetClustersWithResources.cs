using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using YaR.Clouds.Extensions;

namespace YaR.Clouds.Base.Repos.YandexDisk.YadWeb.Models.Media
{
    class YadGetClustersWithResourcesPostModel : YadPostModel
    {
        public string PhotoSliceId { get; }
        public DateTime From { get; }
        public DateTime Till { get; }
        public YadMediaFilter? Filter { get; }

        public YadGetClustersWithResourcesPostModel(string photoSliceId, DateTime from, DateTime till, YadMediaFilter? filter)
        {
            PhotoSliceId = photoSliceId;
            From = from;
            Till = till;
            Filter = filter;
            Name = "getClustersWithResources";
        }

        public override IEnumerable<KeyValuePair<string, string>> ToKvp(int index)
        {
            foreach (var pair in base.ToKvp(index))
                yield return pair;

            yield return new KeyValuePair<string, string>($"photosliceId.{index}", PhotoSliceId);
            if (Filter != null)
                yield return new KeyValuePair<string, string>($"filter.{index}", Filter.Value.ToString().ToLower());

            string clusters = $"{{\"{From.ToUnix().ToString()}_{Till.ToUnix().ToString()}\":{{\"range\":[0,0]}}}}";
            yield return new KeyValuePair<string, string>($"clusters.{index}", clusters);
        }
    }

    //-------------------------------------------------------------------------------------------------------------------------------

    internal class YadGetClustersWithResourcesRequestData : YadModelDataBase
    {
        [JsonProperty("clusters")]
        public Clusters Clusters { get; set; }

        [JsonProperty("resources")]
        public Zresources Resources { get; set; }
    }

    internal class Clusters
    {
        [JsonProperty("fetched")]
        public ClustersFetched[] Fetched { get; set; }

        [JsonProperty("missing")]
        public object[] Missing { get; set; }
    }

    internal class ClustersFetched
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("size")]
        public long Size { get; set; }

        [JsonProperty("items")]
        public Zitem[] Items { get; set; }

        [JsonProperty("albums")]
        public Albums Albums { get; set; }
    }

    internal class Albums
    {
        [JsonProperty("photounlim")]
        public long Photounlim { get; set; }

        [JsonProperty("camera")]
        public long Camera { get; set; }

        [JsonProperty("videos", NullValueHandling = NullValueHandling.Ignore)]
        public long? Videos { get; set; }
    }

    internal class Zitem
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("itemId")]
        public string ItemId { get; set; }

        [JsonProperty("width", NullValueHandling = NullValueHandling.Ignore)]
        public long? Width { get; set; }

        [JsonProperty("height", NullValueHandling = NullValueHandling.Ignore)]
        public long? Height { get; set; }

        [JsonProperty("beauty", NullValueHandling = NullValueHandling.Ignore)]
        public double? Beauty { get; set; }

        [JsonProperty("albums")]
        public YadMediaFilter[] Albums { get; set; }
    }

    internal class Zresources
    {
        [JsonProperty("fetched")]
        public ResourcesFetched[] Fetched { get; set; }

        [JsonProperty("missing")]
        public object[] Missing { get; set; }
    }
    internal class ResourcesFetched
    {
        [JsonProperty("clusterId")]
        public string ClusterId { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("etime")]
        public long Etime { get; set; }

        [JsonProperty("meta")]
        public Zmeta Meta { get; set; }

        [JsonProperty("__type")]
        public string Type { get; set; }

        [JsonProperty("mtime")]
        public long Mtime { get; set; }

        [JsonProperty("path")]
        public string Path { get; set; }

        [JsonProperty("utime")]
        public long Utime { get; set; }

        [JsonProperty("type")]
        public string FetchedType { get; set; }

        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("ctime")]
        public long Ctime { get; set; }
    }

    internal class Zmeta
    {
        [JsonProperty("mimetype")]
        public string Mimetype { get; set; }

        [JsonProperty("drweb")]
        public long Drweb { get; set; }

        [JsonProperty("sizes")]
        public Size[] Sizes { get; set; }

        [JsonProperty("resource_id")]
        public string ResourceId { get; set; }

        [JsonProperty("mediatype")]
        public string Mediatype { get; set; }

        [JsonProperty("etime")]
        public long Etime { get; set; }

        [JsonProperty("storage_type")]
        public YadMediaFilter StorageType { get; set; }

        [JsonProperty("file_id")]
        public string FileId { get; set; }

        [JsonProperty("photoslice_time")]
        public long PhotosliceTime { get; set; }

        [JsonProperty("size")]
        public long Size { get; set; }

        [JsonProperty("short_url", NullValueHandling = NullValueHandling.Ignore)]
        public Uri ShortUrl { get; set; }

        [JsonProperty("public", NullValueHandling = NullValueHandling.Ignore)]
        public long? Public { get; set; }

        [JsonProperty("video_info", NullValueHandling = NullValueHandling.Ignore)]
        public VideoInfo VideoInfo { get; set; }
    }

    //-----------------------------------------------------------------------------------------------------------------------------------
    internal class YadGetClustersWithResourcesRequestParams
    {
        [JsonProperty("photosliceId")]
        public string PhotosliceId { get; set; }

        [JsonProperty("clusters")]
        public string Clusters { get; set; }

        [JsonProperty("filter")]
        [JsonConverter(typeof(FilterConverter))]
        public YadMediaFilter Filter { get; set; }
    }

    internal enum YadMediaFilter { Camera, Photounlim, Videos }

    internal class FilterConverter : JsonConverter
    {
        public override bool CanConvert(Type t) => t == typeof(YadMediaFilter) || t == typeof(YadMediaFilter?);

        public override object ReadJson(JsonReader reader, Type t, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null) return null;
            var value = serializer.Deserialize<string>(reader);
            
            return value switch
            {
                "camera" => YadMediaFilter.Camera,
                "photounlim" => YadMediaFilter.Photounlim,
                "videos" => YadMediaFilter.Videos,
                _ => throw new Exception("Cannot unmarshal type Filter")
            };
        }

        public override void WriteJson(JsonWriter writer, object untypedValue, JsonSerializer serializer)
        {
            if (untypedValue == null)
            {
                serializer.Serialize(writer, null);
                return;
            }
            var value = (YadMediaFilter)untypedValue;
            switch (value)
            {
                case YadMediaFilter.Camera:
                    serializer.Serialize(writer, "camera");
                    return;
                case YadMediaFilter.Photounlim:
                    serializer.Serialize(writer, "photounlim");
                    return;
                case YadMediaFilter.Videos:
                    serializer.Serialize(writer, "videos");
                    return;
            }
            throw new Exception("Cannot marshal type YadMediaFilter");
        }

        //public static readonly FilterConverter Singleton = new FilterConverter();
    }
    //------------------------------



}