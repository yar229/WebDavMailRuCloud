using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net;
using Newtonsoft.Json;
using YaR.Clouds.Base.Repos.YandexDisk.YadWebV2.Models;
using YaR.Clouds.Base.Requests;

namespace YaR.Clouds.Base.Repos.YandexDisk.YadWebV2.Requests
{
    class YaDCommonRequest : BaseRequestJson<YadResponceResult>
    {
        //private static readonly log4net.ILog Logger = log4net.LogManager.GetLogger(typeof(YaDCommonRequest));

        private readonly YadPostData _postData = new();

        private readonly List<object> _outData = new();

        private YadWebAuth YadAuth { get; }

        public YaDCommonRequest(HttpCommonSettings settings, YadWebAuth auth) : base(settings, auth)
        {
            YadAuth = auth;
        }

        protected override HttpWebRequest CreateRequest(string baseDomain = null)
        {
            var request = base.CreateRequest("https://disk.yandex.ru");
            request.Referer = "https://disk.yandex.ru/client/disk";
            return request;
        }

        protected override byte[] CreateHttpContent()
        {
            _postData.Sk = YadAuth.DiskSk;
            _postData.IdClient = YadAuth.Uuid;

            return _postData.CreateHttpContent();
        }

        public YaDCommonRequest With<T, TOut>(T model, out TOut resOUt)
            where T : YadPostModel 
            where TOut : YadResponseModel, new()
        {
            _postData.Models.Add(model);
            _outData.Add(resOUt = new TOut());

            return this;
        }

        protected override string RelationalUri => "/models/?_m=" + _postData.Models
                                                       .Select(m => m.Name)
                                                       .Aggregate((current, next) => current + "," + next);

        protected override RequestResponse<YadResponceResult> DeserializeMessage(NameValueCollection responseHeaders, System.IO.Stream stream)
        {
            using var sr = new StreamReader(stream);

            string text = sr.ReadToEnd();
            //Logger.Debug(text);

            var msg = new RequestResponse<YadResponceResult>
            {
                Ok = true,
                Result = JsonConvert.DeserializeObject<YadResponceResult>(text, new KnownYadModelConverter(_outData))
            };
            return msg;
        }
    }
}