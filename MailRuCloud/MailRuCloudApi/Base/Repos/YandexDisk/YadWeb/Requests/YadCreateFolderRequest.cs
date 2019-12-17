using System.Collections.Generic;
using Newtonsoft.Json;
using YaR.MailRuCloud.Api.Base.Requests;

namespace YaR.MailRuCloud.Api.Base.Repos.YandexDisk.YadWeb.Requests
{
    class YadCreateFolderRequest : YadBaseRequestJson<YadRequestResult<YadCreateFolderRequestData, YadCreateFolderRequestParams>>
    {
        private readonly string _path;

        public YadCreateFolderRequest(HttpCommonSettings settings, YadWebAuth auth, string path)  : base(settings, auth)
        {
            _path = path;
        }

        protected override string RelationalUri => "/models/?_m=do-resource-create-folder";

        protected override IEnumerable<YadPostModel> CreateModels()
        {
            yield return new YadCreateFolderPostModel
            {
                Path = _path
            };
        }
    }

    class YadCreateFolderPostModel : YadPostModel
    {
        public YadCreateFolderPostModel()
        {
            Name = "do-resource-create-folder";
        }

        public string Path { get; set; }
        public bool Force { get; set; }

        public override IEnumerable<KeyValuePair<string, string>> ToKvp(int index)
        {
            foreach (var pair in base.ToKvp(index))
                yield return pair;
            
            yield return new KeyValuePair<string, string>($"id.{index}", WebDavPath.Combine("/disk", Path));
            yield return new KeyValuePair<string, string>($"force.{index}", Force ? "1" : "0");
        }
    }


    public class YadCreateFolderRequestData
    {
    }

    public class YadCreateFolderRequestParams
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("force")]
        public long Force { get; set; }
    }
}