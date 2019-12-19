using YaR.Clouds.Base.Requests;
using YaR.Clouds.Base.Requests.Types;

namespace YaR.Clouds.Base.Repos.MailRuCloud.WebM1.Requests
{
    class SharedListRequest : BaseRequestJson<FolderInfoResult>
    {

        public SharedListRequest(HttpCommonSettings settings, IAuth auth)
            : base(settings, auth)
        {
        }

        protected override string RelationalUri
        {
            get
            {
                var uri = $"/api/m1/folder/shared/links?access_token={Auth.AccessToken}";
                return uri;
            }
        }
    }
}