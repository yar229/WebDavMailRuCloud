using System;
using System.Text;
using YaR.MailRuCloud.Api.Base.Requests;
using YaR.MailRuCloud.Api.Base.Requests.Types;

namespace YaR.MailRuCloud.Api.Base.Repos.MailRuCloud.WebM1.Requests
{

   class RemoveRequest : BaseRequestJson<CommonOperationResult<string>>
    {
        private readonly string _fullPath;

        public RemoveRequest(HttpCommonSettings settings, IAuth auth, string fullPath) 
            : base(settings, auth)
        {
            _fullPath = fullPath;
        }

        protected override string RelationalUri => $"/api/m1/file/remove?access_token={Auth.AccessToken}";

        protected override byte[] CreateHttpContent()
        {
            // path sended using POST cause of unprintable Unicode charactes may exists
            // https://github.com/yar229/WebDavMailRuCloud/issues/54
            string data = $"home={Uri.EscapeDataString(_fullPath)}";
            return Encoding.UTF8.GetBytes(data);
        }
    }
}
