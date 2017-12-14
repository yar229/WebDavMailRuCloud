using System;
using System.Net;
using System.Text;
using YaR.MailRuCloud.Api.Base.Requests.Repo;

namespace YaR.MailRuCloud.Api.Base.Requests.WebV2
{
    class MoveRequest : BaseRequestJson<CopyRequest.Result>
    {
        private readonly string _sourceFullPath;
        private readonly string _destinationPath;

        public MoveRequest(HttpCommonSettings settings, IAuth auth, string sourceFullPath, string destinationPath) 
            : base(settings, auth)
        {
            _sourceFullPath = sourceFullPath;
            _destinationPath = destinationPath;
        }

        protected override string RelationalUri => "/api/v2/file/move";

        protected override byte[] CreateHttpContent()
        {
            var data = Encoding.UTF8.GetBytes(string.Format("home={0}&api={1}&token={2}&email={3}&x-email={3}&conflict=rename&folder={4}",
                Uri.EscapeDataString(_sourceFullPath), 2, Auth.AccessToken, Auth.Login, Uri.EscapeDataString(_destinationPath)));

            return data;
        }
    }
}
