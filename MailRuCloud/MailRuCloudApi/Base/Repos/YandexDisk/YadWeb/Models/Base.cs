using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using Newtonsoft.Json;

namespace YaR.MailRuCloud.Api.Base.Repos.YandexDisk.YadWeb.Models
{
    class YadPostData
    {
        public string Sk { get; set; }
        public string IdClient { get; set; }
        public List<YadPostModel> Models { get; set; } = new List<YadPostModel>();

        public byte[] CreateHttpContent()
        {
            var keyValues = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("sk", Sk),
                new KeyValuePair<string, string>("idClient", IdClient)
            };

            keyValues.AddRange(Models.SelectMany((model, i) => model.ToKvp(i)));

            FormUrlEncodedContent z = new FormUrlEncodedContent(keyValues);
            return z.ReadAsByteArrayAsync().Result;
        }
    }

    abstract class YadPostModel
    {
        public virtual IEnumerable<KeyValuePair<string, string>> ToKvp(int index)
        {
            yield return new KeyValuePair<string, string>($"_model.{index}", Name);
        }

        public string Name { get; set; }
    }







    public class YadResponceResult
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
        public List<YadResponceModel> Models { get; set; }
    }

    public class YadResponceModel
    {
        [JsonProperty("model")]
        public string ModelName { get; set; }

        [JsonProperty("error")]
        public YadResponceError Error { get; set; }
    }


    public class YadResponceModel<TData, TParams> : YadResponceModel
    {
        [JsonProperty("params")]
        public TParams Params { get; set; }

        [JsonProperty("data")]
        public TData Data { get; set; }
    }

    public class YadResponceError
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("message")]
        public string Message { get; set; }
    }
}
