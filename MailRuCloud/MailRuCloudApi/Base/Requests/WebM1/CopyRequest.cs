using System;
using System.Text;
using YaR.MailRuCloud.Api.Base.Auth;
using YaR.MailRuCloud.Api.Base.Requests.Types;

namespace YaR.MailRuCloud.Api.Base.Requests.WebM1
{
    class CopyRequest : BaseRequestJson<CommonOperationResult<string>>
    {
        private readonly string _sourceFullPath;
        private readonly string _destinationPath;
        private readonly ConflictResolver _conflictResolver;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="settings"></param>
        /// <param name="auth"></param>
        /// <param name="sourceFullPath"></param>
        /// <param name="destinationPath">(without item name)</param>
        /// <param name="conflictResolver"></param>
        public CopyRequest(HttpCommonSettings settings, IAuth auth, string sourceFullPath, string destinationPath, ConflictResolver? conflictResolver = null) 
            : base(settings, auth)
        {
            _sourceFullPath = sourceFullPath;
            _destinationPath = destinationPath;
            _conflictResolver = conflictResolver ?? ConflictResolver.Rename;
        }

        protected override string RelationalUri => $"/api/m1/file/copy?access_token={Auth.AccessToken}";

        protected override byte[] CreateHttpContent()
        {
            var data = $"home={Uri.EscapeDataString(_sourceFullPath)}&email={Auth.Login}&x-email={Auth.Login}&conflict={_conflictResolver}&folder={Uri.EscapeDataString(_destinationPath)}";
            return Encoding.UTF8.GetBytes(data);
        }
    }
}