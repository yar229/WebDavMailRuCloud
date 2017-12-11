using System;
using System.Net;
using System.Text;
using YaR.MailRuCloud.Api.Base.Requests.Repo;

namespace YaR.MailRuCloud.Api.Base.Requests.WebM1
{
    class MoveRequest : BaseRequestJson<WebV2.CopyRequest.Result>
    {
        private readonly string _sourceFullPath;
        private readonly string _destinationPath;

        public MoveRequest(IWebProxy proxy, IAuth auth, string sourceFullPath, string destinationPath) : base(proxy, auth)
        {
            _sourceFullPath = sourceFullPath;
            _destinationPath = destinationPath;
        }

        protected override string RelationalUri => "/api/m1/file/move";

        protected override byte[] CreateHttpContent()
        {
            var data = Encoding.UTF8.GetBytes(string.Format("home={0}&api={1}&access_token={2}&email={3}&x-email={3}&conflict=rename&folder={4}",
                Uri.EscapeDataString(_sourceFullPath), 2, Auth.AccessToken, Auth.Login, Uri.EscapeDataString(_destinationPath)));

            return data;
        }
    }
}
