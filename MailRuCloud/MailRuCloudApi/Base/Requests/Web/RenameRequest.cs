using System;
using System.Text;

namespace YaR.MailRuCloud.Api.Base.Requests.Web
{
    
    class RenameRequest : BaseRequestJson<RenameRequest.Result>
    {
        private readonly string _token;
        private readonly string _fullPath;
        private readonly string _newName;

        public RenameRequest(CloudApi cloudApi, string token, string fullPath, string newName) : base(cloudApi)
        {
            _token = token;
            _fullPath = fullPath;
            _newName = newName;
        }

        protected override string RelationalUri => "/api/v2/file/rename";

        protected override byte[] CreateHttpContent()
        {
            var data = string.Format("home={0}&api={1}&token={2}&email={3}&x-email={3}&conflict=rename&name={4}", Uri.EscapeDataString(_fullPath),
                2, _token, CloudApi.Account.Credentials.Login, Uri.EscapeDataString(_newName));
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
