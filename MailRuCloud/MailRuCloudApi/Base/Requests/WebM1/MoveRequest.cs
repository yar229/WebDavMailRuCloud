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

        public MoveRequest(HttpCommonSettings settings, IAuth auth, string sourceFullPath, string destinationPath) 
            : base(settings, auth)
        {
            _sourceFullPath = sourceFullPath;
            _destinationPath = destinationPath;
        }

        protected override string RelationalUri => $"/api/m1/file/move?access_token={Auth.AccessToken}";

        protected override byte[] CreateHttpContent()
        {
            var data = $"home={Uri.EscapeDataString(_sourceFullPath)}&email={Auth.Login}&conflict=rename&folder={Uri.EscapeDataString(_destinationPath)}";
            return Encoding.UTF8.GetBytes(data);
        }
    }
}
