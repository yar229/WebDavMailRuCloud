using System;
using System.Text;
using YaR.Clouds.Base.Requests;
using YaR.Clouds.Base.Requests.Types;

namespace YaR.Clouds.Base.Repos.MailRuCloud.WebM1.Requests
{
    class UnpublishRequest : BaseRequestJson<CommonOperationResult<string>>
    {
        private readonly string _publicLink;

        public UnpublishRequest(IRequestRepo repo, HttpCommonSettings settings, IAuth auth, string publicLink) 
            : base(settings, auth)
        {
            _publicLink = publicLink;

            if (repo.PublicBaseUrlDefault.Length > 0 && 
                _publicLink.StartsWith(repo.PublicBaseUrlDefault, StringComparison.InvariantCultureIgnoreCase))
                _publicLink = _publicLink.Remove(0, repo.PublicBaseUrlDefault.Length);
        }

        protected override string RelationalUri => $"/api/m1/file/unpublish?access_token={Auth.AccessToken}";

        protected override byte[] CreateHttpContent()
        {
            var data = $"weblink={Uri.EscapeDataString(_publicLink)}&email={Auth.Login}&x-email={Auth.Login}";
            return Encoding.UTF8.GetBytes(data);
        }
    }
}