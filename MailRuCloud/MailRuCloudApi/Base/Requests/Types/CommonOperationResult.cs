using Newtonsoft.Json;

namespace YaR.MailRuCloud.Api.Base.Requests.Types
{
    internal class CommonOperationResult<T>
    {
        [JsonProperty("email")]
        public string Email { get; set; }
        [JsonProperty("body")]
        public T Body { get; set; }
        [JsonProperty("time")]
        public long Time { get; set; }
        [JsonProperty("status")]
        public int Status { get; set; }
    }
}