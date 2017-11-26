using System;
using System.Text;

namespace YaR.MailRuCloud.Api.Base.Requests.Web
{
    class PublishRequest : BaseRequestJson<PublishRequest.Result>
    {
        private readonly string _fullPath;

        public PublishRequest(RequestInit init, string fullPath) : base(init)
        {
            _fullPath = fullPath;
        }

        protected override string RelationalUri => "/api/v2/file/publish";

        protected override byte[] CreateHttpContent()
        {
            var data = string.Format("home={0}&api={1}&token={2}&email={3}&x-email={3}", Uri.EscapeDataString(_fullPath),
                2, Init.Token, Init.Login);
            return Encoding.UTF8.GetBytes(data);
        }

        public class Result
        {
            public string email { get; set; }
            public string body { get; set; }
            public long time { get; set; }
            public int status { get; set; }
        }
    }
}