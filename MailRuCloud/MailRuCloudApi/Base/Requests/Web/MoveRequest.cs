using System;
using System.Text;
using YaR.MailRuCloud.Api.Base.Requests.Types;

namespace YaR.MailRuCloud.Api.Base.Requests.Web
{
    class MoveRequest : BaseRequestJson<CopyRequest.Result>
    {
        private readonly string _token;
        private readonly string _sourceFullPath;
        private readonly string _destinationPath;

        public MoveRequest(CloudApi cloudApi, string token, string sourceFullPath, string destinationPath) : base(cloudApi)
        {
            _token = token;
            _sourceFullPath = sourceFullPath;
            _destinationPath = destinationPath;
        }

        protected override string RelationalUri => "/api/v2/file/move";

        protected override byte[] CreateHttpContent()
        {
            var data = Encoding.UTF8.GetBytes(string.Format("home={0}&api={1}&token={2}&email={3}&x-email={3}&conflict=rename&folder={4}",
                Uri.EscapeDataString(_sourceFullPath), 2, _token, CloudApi.Account.Credentials.Login, Uri.EscapeDataString(_destinationPath)));

            return data;
        }
    }
}
