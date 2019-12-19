using System;
using System.Text;
using YaR.Clouds.Base.Requests;
using YaR.Clouds.Base.Requests.Types;

namespace YaR.Clouds.Base.Repos.MailRuCloud.WebM1.Requests
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
