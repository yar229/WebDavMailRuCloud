using System;
using System.Net;
using System.Text;
using YaR.MailRuCloud.Api.Base.Requests.Repo;

namespace YaR.MailRuCloud.Api.Base.Requests.WebM1
{
    
    class RenameRequest : BaseRequestJson<WebV2.RenameRequest.Result>
    {
        private readonly string _fullPath;
        private readonly string _newName;

        public RenameRequest(IWebProxy proxy, IAuth auth, string fullPath, string newName) : base(proxy, auth)
        {
            _fullPath = fullPath;
            _newName = newName;
        }

        protected override string RelationalUri => "/api/m1/file/rename";

        protected override byte[] CreateHttpContent()
        {
            var data = string.Format("home={0}&api={1}&access_token={2}&email={3}&x-email={3}&conflict=rename&name={4}", Uri.EscapeDataString(_fullPath),
                2, Auth.AccessToken, Auth.Login, Uri.EscapeDataString(_newName));
            return Encoding.UTF8.GetBytes(data);
        }
    }
}
