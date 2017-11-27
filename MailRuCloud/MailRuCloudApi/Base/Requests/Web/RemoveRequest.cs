using System;
using System.Text;

namespace YaR.MailRuCloud.Api.Base.Requests.Web
{

   class RemoveRequest : BaseRequestJson<RemoveRequest.Result>
    {
        private readonly string _fullPath;

        public RemoveRequest(RequestInit init, string fullPath) : base(init)
        {
            _fullPath = fullPath;
        }

        protected override string RelationalUri => "/api/v2/file/remove";

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
