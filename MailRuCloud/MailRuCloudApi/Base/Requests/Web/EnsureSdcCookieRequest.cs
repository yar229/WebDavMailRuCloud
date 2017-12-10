using System.Net;
using YaR.MailRuCloud.Api.Base.Requests.Repo;

namespace YaR.MailRuCloud.Api.Base.Requests.Web
{
    class EnsureSdcCookieRequest : BaseRequestString
    {
        public EnsureSdcCookieRequest(IWebProxy proxy, IAuth auth) : base(proxy, auth)
        {
        }

        protected override HttpWebRequest CreateRequest(string baseDomain = null)
        {
            var request = base.CreateRequest(ConstSettings.AuthDomain);
            request.Accept = ConstSettings.DefaultAcceptType;
            return request;
        }

        protected override string RelationalUri
        {
            get
            {
                var uri = $"/sdc?from={ConstSettings.CloudDomain}/home";
                return uri;
            }
        }
    }
}
