using System;
using System.Text;

namespace YaR.MailRuCloud.Api.Base.Requests.Web
{
    class UnpublishRequest : BaseRequestJson<UnpublishRequest.Result>
    {
        private readonly string _publicLink;

        public UnpublishRequest(RequestInit init, string publicLink) : base(init)
        {
            _publicLink = publicLink;
        }

        protected override string RelationalUri => "/api/v2/file/unpublish";

        protected override byte[] CreateHttpContent()
        {
            var data = string.Format("weblink={0}&api={1}&token={2}&email={3}&x-email={3}", Uri.EscapeDataString(_publicLink),
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