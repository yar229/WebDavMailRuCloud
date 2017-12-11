using System;
using System.Net;
using System.Text;
using YaR.MailRuCloud.Api.Base.Requests.Repo;

namespace YaR.MailRuCloud.Api.Base.Requests.WebM1
{
   class CreateFolderRequest : BaseRequestJson<WebV2.CreateFolderRequest.Result>
    {
        private readonly string _fullPath;

        public CreateFolderRequest(IWebProxy proxy, IAuth auth, string fullPath) : base(proxy, auth)
        {
            _fullPath = fullPath;
        }

        protected override string RelationalUri => "/api/m1/folder/add";

        protected override byte[] CreateHttpContent()
        {
            var data = $"home={Uri.EscapeDataString(_fullPath)}&conflict=rename&api={2}&access_token={Auth.AccessToken}";
            return Encoding.UTF8.GetBytes(data);
        }
    }
}
