using System;
using System.Text;
using YaR.MailRuCloud.Api.Base.Auth;
using YaR.MailRuCloud.Api.Base.Requests.Types;

namespace YaR.MailRuCloud.Api.Base.Requests.WebM1
{
    class RenameRequest : BaseRequestJson<CommonOperationResult<string>>
    {
        private readonly string _fullPath;
        private readonly string _newName;

        public RenameRequest(HttpCommonSettings settings, IAuth auth, string fullPath, string newName) 
            : base(settings, auth)
        {
            _fullPath = fullPath;
            _newName = newName;
        }

        protected override string RelationalUri => $"/api/m1/file/rename?access_token={Auth.AccessToken}";

        protected override byte[] CreateHttpContent()
        {
            var data = $"home={Uri.EscapeDataString(_fullPath)}&email={Auth.Login}&conflict=rename&name={Uri.EscapeDataString(_newName)}";
            return Encoding.UTF8.GetBytes(data);
        }
    }
}
