using Newtonsoft.Json;

namespace YaR.MailRuCloud.Api.Base.Requests.Types
{
    internal class DownloadTokenResult : CommonOperationResult<DownloadTokenResult.DownloadTokenBody>
    {
        public class DownloadTokenBody
        {
            [JsonProperty("token")]
            public string Token { get; set; }
        }
    }
}