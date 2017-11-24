using System.Net;

namespace YaR.MailRuCloud.Api.Base.Requests.Web
{
    class EnsureSdcCookieRequest : BaseRequest<string>
    {
        public EnsureSdcCookieRequest(CloudApi cloudApi) : base(cloudApi)
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
