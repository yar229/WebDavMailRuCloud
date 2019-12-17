using System.Collections.Generic;
using Newtonsoft.Json;
using YaR.MailRuCloud.Api.Base.Requests;

namespace YaR.MailRuCloud.Api.Base.Repos.YandexDisk.YadWeb.Requests
{
    class YadMoveRequest : YadBaseRequestJson<YadRequestResult<YadMoveRequestData, YadMoveRequestParams>>
    {
        private readonly string _sourcePath;
        private readonly string _destPath;

        public YadMoveRequest(HttpCommonSettings settings, YadWebAuth auth, string sourcePath, string destPath)  : base(settings, auth)
        {
            _sourcePath = sourcePath;
            _destPath = destPath;
        }

        protected override string RelationalUri => "/models/?_m=do-resource-move";

        protected override IEnumerable<YadPostModel> CreateModels()
        {
            yield return new YadMovePostModel
            {
                Source = _sourcePath,
                Destination = _destPath,
                Force = true
            };
        }
    }

    class YadMovePostModel : YadPostModel
    {
        public YadMovePostModel()
        {
            Name = "do-resource-move";
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

    internal class YadMoveRequestData
    {
        [JsonProperty("at_version")]
        public long AtVersion { get; set; }

        [JsonProperty("oid")]
        public string Oid { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }
    }

    internal class YadMoveRequestParams
    {
        [JsonProperty("src")]
        public string Src { get; set; }

        [JsonProperty("dst")]
        public string Dst { get; set; }

        [JsonProperty("force")]
        public long Force { get; set; }
    }
}