using System.Collections.Generic;
using Newtonsoft.Json;
using YaR.MailRuCloud.Api.Base.Requests;

namespace YaR.MailRuCloud.Api.Base.Repos.YandexDisk.YadWeb.Requests
{
    class YadCopyRequest : YadBaseRequestJson<YadRequestResult<YadCopyRequestData, YadCopyRequestParams>>
    {
        private readonly string _sourcePath;
        private readonly string _destPath;

        public YadCopyRequest(HttpCommonSettings settings, YadWebAuth auth, string sourcePath, string destPath)  : base(settings, auth)
        {
            _sourcePath = sourcePath;
            _destPath = destPath;
        }

        protected override string RelationalUri => "/models/?_m=do-resource-copy";

        protected override IEnumerable<YadPostModel> CreateModels()
        {
            yield return new YadCopyPostModel
            {
                Source = _sourcePath,
                Destination = _destPath,
                Force = true
            };
        }
    }


    class YadCopyPostModel : YadPostModel
    {
        public YadCopyPostModel()
        {
            Name = "do-resource-copy";
        }

        public string Source { get; set; }
        public string Destination { get; set; }
        public bool Force { get; set; }

        public override IEnumerable<KeyValuePair<string, string>> ToKvp(int index)
        {
            foreach (var pair in base.ToKvp(index))
                yield return pair;
            
            yield return new KeyValuePair<string, string>($"src.{index}", WebDavPath.Combine("/disk", Source));
            yield return new KeyValuePair<string, string>($"dst.{index}", WebDavPath.Combine("/disk", Destination));
            yield return new KeyValuePair<string, string>($"force.{index}", Force ? "1" : "0");
        }
    }



    class YadCopyRequestData
    {
        [JsonProperty("at_version")]
        public long AtVersion { get; set; }

        [JsonProperty("oid")]
        public string Oid { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }
    }

    class YadCopyRequestParams
    {
        [JsonProperty("src")]
        public string Src { get; set; }

        [JsonProperty("dst")]
        public string Dst { get; set; }

        [JsonProperty("force")]
        public long Force { get; set; }
    }
}