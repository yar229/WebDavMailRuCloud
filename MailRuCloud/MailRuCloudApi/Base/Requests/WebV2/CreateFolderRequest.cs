using System;
using System.Net;
using System.Text;
using YaR.MailRuCloud.Api.Base.Requests.Repo;

namespace YaR.MailRuCloud.Api.Base.Requests.WebV2
{
   class CreateFolderRequest : BaseRequestJson<CreateFolderRequest.Result>
    {
        private readonly string _fullPath;

        public CreateFolderRequest(IWebProxy proxy, IAuth auth, string fullPath) : base(proxy, auth)
        {
            _fullPath = fullPath;
        }

        protected override string RelationalUri => "/api/v2/folder/add";

        protected override byte[] CreateHttpContent()
        {
            var data = $"home={Uri.EscapeDataString(_fullPath)}&conflict=rename&api={2}&token={Auth.AccessToken}";
            return Encoding.UTF8.GetBytes(data);
        }


        internal class Result
        {
            public string email { get; set; }
            public string body { get; set; }
            public long time { get; set; }
            public int status { get; set; }
        }
    }
}
