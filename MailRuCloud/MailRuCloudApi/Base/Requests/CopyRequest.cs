using System;
using System.Text;
using YaR.MailRuCloud.Api.Base.Requests.Types;

namespace YaR.MailRuCloud.Api.Base.Requests
{
    class CopyRequest : BaseRequest<MoveOrCopyResult>
    {
        private readonly string _sourceFullPath;
        private readonly string _destinationPath;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="cloudApi"></param>
        /// <param name="sourceFullPath"></param>
        /// <param name="destinationPath">(without item name)</param>
        public CopyRequest(CloudApi cloudApi, string sourceFullPath, string destinationPath) : base(cloudApi)
        {
            _sourceFullPath = sourceFullPath;
            _destinationPath = destinationPath;
        }

        protected override string RelationalUri => "/api/v2/file/copy";

        protected override byte[] CreateHttpContent()
        {
            var data = Encoding.UTF8.GetBytes(string.Format("home={0}&api={1}&token={2}&email={3}&x-email={3}&conflict=rename&folder={4}",
                Uri.EscapeDataString(_sourceFullPath), 2, CloudApi.Account.AuthToken, CloudApi.Account.LoginName, 
                Uri.EscapeDataString(_destinationPath)));

            return data;
        }
    }
}