using System;
using System.Text;
using YaR.MailRuCloud.Api.Base.Requests;
using YaR.MailRuCloud.Api.Base.Requests.Types;

namespace YaR.MailRuCloud.Api.Base.Repos.WebV2.Requests
{
    class PublishRequest : BaseRequestJson<CommonOperationResult<string>>
    {
        private readonly string _fullPath;

        public PublishRequest(HttpCommonSettings settings, IAuth auth, string fullPath) : base(settings, auth)
        {
            _fullPath = fullPath;
        }

        protected override string RelationalUri => "/api/v2/file/publish";

        protected override byte[] CreateHttpContent()
        {
            var data = string.Format("home={0}&api={1}&token={2}&email={3}&x-email={3}", Uri.EscapeDataString(_fullPath),
                2, Auth.AccessToken, Auth.Login);
            return Encoding.UTF8.GetBytes(data);
        }
    }
}