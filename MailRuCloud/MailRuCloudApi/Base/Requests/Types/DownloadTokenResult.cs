using Newtonsoft.Json;

namespace YaR.Clouds.Base.Requests.Types
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