using System;
using System.Text;
using YaR.Clouds.Base.Requests;
using YaR.Clouds.Base.Requests.Types;

namespace YaR.Clouds.Base.Repos.MailRuCloud.WebV2.Requests
{
    class UnpublishRequest : BaseRequestJson<CommonOperationResult<string>>
    {
        private readonly string _publicLink;

        public UnpublishRequest(HttpCommonSettings settings, IAuth auth, string publicLink) : base(settings, auth)
        {
            _publicLink = publicLink;
        }

        protected override string RelationalUri => "/api/v2/file/unpublish";

        protected override byte[] CreateHttpContent()
        {
            var data = string.Format("weblink={0}&api={1}&token={2}&email={3}&x-email={3}", Uri.EscapeDataString(_publicLink),
                2, Auth.AccessToken, Auth.Login);
            return Encoding.UTF8.GetBytes(data);
        }
    }
}