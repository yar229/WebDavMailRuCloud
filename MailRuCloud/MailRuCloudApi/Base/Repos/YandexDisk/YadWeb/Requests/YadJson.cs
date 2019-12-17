using System.Collections.Generic;
using Newtonsoft.Json;

namespace YaR.MailRuCloud.Api.Base.Repos.YandexDisk.YadWeb.Requests
{
    public class YadRequestResult<TData, TParams>
    {
        [JsonProperty("uid")]
        public long Uid { get; set; }

        [JsonProperty("login")]
        public string Login { get; set; }

        [JsonProperty("sk")]
        public string Sk { get; set; }

        [JsonProperty("version")]
        public string Version { get; set; }

        [JsonProperty("models")]
        public List<YadModel<TData, TParams>> Models { get; set; }
    }

    public class YadModel<TData, TParams>
    {
        [JsonProperty("model")]
        public string ModelName { get; set; }

        [JsonProperty("params")]
        public TParams Params { get; set; }

        [JsonProperty("data")]
        public TData Data { get; set; }

        [JsonProperty("error")]
        public Error Error { get; set; }
    }

    public class Error
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("message")]
        public string Message { get; set; }
    }
}
