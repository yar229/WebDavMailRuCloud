using System.IO;
using YaR.Clouds.Base.Repos.MailRuCloud.Mobile.Requests.Types;
using YaR.Clouds.Base.Requests;

namespace YaR.Clouds.Base.Repos.MailRuCloud.Mobile.Requests
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
