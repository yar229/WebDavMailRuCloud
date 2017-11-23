using System;
using System.Text;
using YaR.MailRuCloud.Api.Base.Requests.Types;

namespace YaR.MailRuCloud.Api.Base.Requests.Web
{
    //TODO: refact, make MoveRequest
    class MoveOrCopyRequest : BaseRequest<MoveOrCopyResult>
    {
        private readonly string _sourceFullPath;
        private readonly string _destinationPath;
        private readonly bool _move;

        public MoveOrCopyRequest(CloudApi cloudApi, string sourceFullPath, string destinationPath, bool move) : base(cloudApi)
        {
            _sourceFullPath = sourceFullPath;
            _destinationPath = destinationPath;
            _move = move;
        }

        protected override string RelationalUri
        {
            get
            {
                var uri = $"/api/v2/file/{(_move ? "move" : "copy")}";
                return uri;
            }
        }

        protected override byte[] CreateHttpContent()
        {
            var data = Encoding.UTF8.GetBytes(string.Format("home={0}&api={1}&token={2}&email={3}&x-email={3}&conflict=rename&folder={4}",
                Uri.EscapeDataString(_sourceFullPath), 2, CloudApi.Account.AuthToken, CloudApi.Account.Credentials.Login, Uri.EscapeDataString(_destinationPath)));

            return data;
        }
    }
}
