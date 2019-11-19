using System.Collections.Generic;
using Newtonsoft.Json;

namespace YaR.MailRuCloud.Api.Base.Requests.Types
{
    internal class ShardInfoRequestResult : CommonOperationResult<ShardInfoRequestResult.ShardInfoResultBody>
    {
        public class ShardInfoResultBody
        {
            [JsonProperty("video")]
            public List<ShardSection> Video { get; set; }
            [JsonProperty("view_direct")]
            public List<ShardSection> ViewDirect { get; set; }
            [JsonProperty("weblink_view")]
            public List<ShardSection> WeblinkView { get; set; }
            [JsonProperty("weblink_video")]
            public List<ShardSection> WeblinkVideo { get; set; }
            [JsonProperty("weblink_video")]
            public List<ShardSection> WeblinkGet { get; set; }
            [JsonProperty("weblink_thumbnails")]
            public List<ShardSection> WeblinkThumbnails { get; set; }
            [JsonProperty("auth")]
            public List<ShardSection> Auth { get; set; }
            [JsonProperty("view")]
            public List<ShardSection> View { get; set; }
            [JsonProperty("get")]
            public List<ShardSection> Get { get; set; }
            [JsonProperty("upload")]
            public List<ShardSection> Upload { get; set; }
            [JsonProperty("thumbnails")]
            public List<ShardSection> Thumbnails { get; set; }

            public class ShardSection
            {
                [JsonProperty("count")]
                public string Count { get; set; }
                [JsonProperty("url")]
                public string Url { get; set; }
            }
        }
    }
}
