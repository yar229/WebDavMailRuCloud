using System.Collections.Generic;
using Newtonsoft.Json;
using YaR.MailRuCloud.Api.Base.Requests;

namespace YaR.MailRuCloud.Api.Base.Repos.YadWeb.Requests
{
    class YadGetResourceUrlRequest : YadBaseRequestJson<YadRequestResult<ResourceUrlData, ResourceUrlParams>>
    {
        private readonly string _path;

        public YadGetResourceUrlRequest(HttpCommonSettings settings, YadWebAuth auth, string path)  : base(settings, auth)
        {
            _path = path;
        }

        protected override string RelationalUri => "/models/?_m=do-get-resource-url";

        protected override IEnumerable<YadPostModel> CreateModels()
        {
            yield return new YadGetResourceUrlPostModel
            {
                Id = WebDavPath.Combine("/disk", _path)
            };
        }
    }

    class YadGetResourceUrlPostModel : YadPostModel
    {
        public YadGetResourceUrlPostModel()
        {
            Name = "do-get-resource-url";
        }

        public string Id { get; set; }

        public override IEnumerable<KeyValuePair<string, string>> ToKvp(int index)
        {
            foreach (var pair in base.ToKvp(index))
                yield return pair;
            
            yield return new KeyValuePair<string, string>($"id.{index}", Id);
        }
    }

    internal class ResourceUrlData
    {
        [JsonProperty("digest")]
        public string Digest { get; set; }

        [JsonProperty("file")]
        public string File { get; set; }
    }

    internal class ResourceUrlParams
    {
        [JsonProperty("id")]
        public string Id { get; set; }
    }
}