using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using YaR.MailRuCloud.Api.Base.Requests;

namespace YaR.MailRuCloud.Api.Base.Repos.YadWeb.Requests
{
    abstract class YadBaseRequestJson<T> : BaseRequestJson<T> where T: class
    {
        public YadBaseRequestJson(HttpCommonSettings settings, YadWebAuth auth) : base(settings, auth)
        {
            YadAuth = auth;
        }

        protected override HttpWebRequest CreateRequest(string baseDomain = null)
        {
            var request = base.CreateRequest("https://disk.yandex.ru");
            request.Referer = "https://disk.yandex.ru/client/disk";
            return request;
        }

        private YadWebAuth YadAuth { get; }

        protected abstract IEnumerable<YadPostModel> CreateModels();


        protected override byte[] CreateHttpContent()
        {
            var pd = new YadPostData
            {
                Sk = YadAuth.DiskSk,
                IdClient = YadAuth.Uuid, 
                Models = CreateModels().ToList()
            };

            return pd.CreateHttpContent();
        }
    }

    class YadPostData
    {
        public string Sk { get; set; }
        public string IdClient { get; set; }
        public List<YadPostModel> Models { get; set; } // = new List<YadPostModel>();

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
}