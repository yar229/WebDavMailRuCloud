using System.Collections.Generic;
using Newtonsoft.Json;
using YaR.MailRuCloud.Api.Base.Requests;

namespace YaR.MailRuCloud.Api.Base.Repos.YandexDisk.YadWeb.Requests
{
    class YadDeleteRequest : YadBaseRequestJson<YadRequestResult<YadDeleteRequestData, YadDeleteRequestParams>>
    {
        private readonly string _path;

        public YadDeleteRequest(HttpCommonSettings settings, YadWebAuth auth, string path)  : base(settings, auth)
        {
            _path = path;
        }

        protected override string RelationalUri => "/models/?_m=do-resource-delete";

        protected override IEnumerable<YadPostModel> CreateModels()
        {
            yield return new YadDeletePostModel
            {
                Path = _path
            };
        }
    }


    class YadDeletePostModel : YadPostModel
    {
        public YadDeletePostModel()
        {
            Name = "do-resource-delete";
        }

        public string Path { get; set; }

        public override IEnumerable<KeyValuePair<string, string>> ToKvp(int index)
        {
            foreach (var pair in base.ToKvp(index))
                yield return pair;
            
            yield return new KeyValuePair<string, string>($"id.{index}", WebDavPath.Combine("/disk", Path));
        }
    }

    public class YadDeleteRequestData
    {
        [JsonProperty("at_version")]
        public long AtVersion { get; set; }

        [JsonProperty("oid")]
        public string Oid { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }
    }

    public class YadDeleteRequestParams
    {
        [JsonProperty("id")]
        public string Id { get; set; }
    }
}