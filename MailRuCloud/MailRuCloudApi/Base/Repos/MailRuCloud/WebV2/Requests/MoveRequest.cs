using System;
using System.Text;
using YaR.Clouds.Base.Requests;
using YaR.Clouds.Base.Requests.Types;

namespace YaR.Clouds.Base.Repos.MailRuCloud.WebV2.Requests
{
    class MoveRequest : BaseRequestJson<CommonOperationResult<string>>
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
