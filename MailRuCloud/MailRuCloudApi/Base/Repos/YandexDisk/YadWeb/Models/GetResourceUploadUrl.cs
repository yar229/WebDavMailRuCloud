using System.Collections.Generic;
using Newtonsoft.Json;

namespace YaR.Clouds.Base.Repos.YandexDisk.YadWeb.Models
{
    class YadGetResourceUploadUrlPostModel : YadPostModel
    {
        public YadGetResourceUploadUrlPostModel(string path, long size, bool force = true)
        {
            Name = "do-resource-upload-url";
            Destination = path;
            Size = size;
            Force = force;
        }

        public string Destination { get; set; }
        public bool Force { get; set; }
        public long Size { get; set; }
        public string Md5 { get; set; }
        public string Sha256 { get; set; }

        public override IEnumerable<KeyValuePair<string, string>> ToKvp(int index)
        {
            foreach (var pair in base.ToKvp(index))
                yield return pair;
            
            yield return new KeyValuePair<string, string>($"dst.{index}", WebDavPath.Combine("/disk", Destination));
            yield return new KeyValuePair<string, string>($"force.{index}", Force ? "1" : "0");
            yield return new KeyValuePair<string, string>($"size.{index}", Size.ToString());
            yield return new KeyValuePair<string, string>($"md5.{index}", Md5);
            yield return new KeyValuePair<string, string>($"sha256.{index}", Sha256);
        }
    }

    internal class ResourceUploadUrlData : YadModelDataBase
    {
        [JsonProperty("at_version")]
        public long AtVersion { get; set; }

        [JsonProperty("upload_url")]
        public string UploadUrl { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("oid")]
        public string Oid { get; set; }
    }

    internal class ResourceUploadUrlParams
    {
        [JsonProperty("dst")]
        public string Dst { get; set; }

        [JsonProperty("force")]
        public long Force { get; set; }

        [JsonProperty("size")]
        public long Size { get; set; }

        [JsonProperty("md5")]
        public string Md5 { get; set; }

        [JsonProperty("sha256")]
        public string Sha256 { get; set; }
    }
}