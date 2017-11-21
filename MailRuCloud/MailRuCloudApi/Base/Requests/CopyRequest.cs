using System;
using System.Text;
using YaR.MailRuCloud.Api.Base.Requests.Types;

namespace YaR.MailRuCloud.Api.Base.Requests
{
    class CopyRequest : BaseRequest<MoveOrCopyResult>
    {
        private readonly string _sourceFullPath;
        private readonly string _destinationPath;
        private readonly ConflictResolver _conflictResolver;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="cloudApi"></param>
        /// <param name="sourceFullPath"></param>
        /// <param name="destinationPath">(without item name)</param>
        /// <param name="conflictResolver"></param>
        public CopyRequest(CloudApi cloudApi, string sourceFullPath, string destinationPath, ConflictResolver? conflictResolver = null) : base(cloudApi)
        {
            _sourceFullPath = sourceFullPath;
            _destinationPath = destinationPath;
            _conflictResolver = conflictResolver ?? ConflictResolver.Rename;
        }

        protected override string RelationalUri => "/api/v2/file/copy";

        protected override byte[] CreateHttpContent()
        {
            var data = Encoding.UTF8.GetBytes(string.Format("home={0}&api={1}&token={2}&email={3}&x-email={3}&conflict={4}&folder={5}",
                Uri.EscapeDataString(_sourceFullPath), 2, CloudApi.Account.AuthToken, CloudApi.Account.Credentials.Login, 
                _conflictResolver,
                Uri.EscapeDataString(_destinationPath)));

            return data;
        }
    }
}