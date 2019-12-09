using System.IO;
using YaR.MailRuCloud.Api.Base.Repos.Mobile.Requests.Types;
using YaR.MailRuCloud.Api.Base.Requests;

namespace YaR.MailRuCloud.Api.Base.Repos.Mobile.Requests
{
    abstract class BaseRequestMobile<T> : BaseRequest<ResponseBodyStream, T> where T : class 
    {
        private readonly string _metaServer;

        protected BaseRequestMobile(HttpCommonSettings settings, IAuth auth, string metaServer) : base(settings, auth)
        {
            _metaServer = metaServer;
        }

        protected override string RelationalUri => $"{_metaServer}?token={Auth.AccessToken}&client_id={Settings.ClientId}";

        protected override ResponseBodyStream Transport(Stream stream)
        {
            return new ResponseBodyStream(stream);
        }
    }
}
