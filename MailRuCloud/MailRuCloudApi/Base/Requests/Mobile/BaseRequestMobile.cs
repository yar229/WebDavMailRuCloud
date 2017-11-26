using System.IO;

namespace YaR.MailRuCloud.Api.Base.Requests.Mobile
{
    abstract class BaseRequestMobile<T> : BaseRequest<ResponseBodyStream, T> where T : class 
    {
        private readonly string _token;
        private readonly string _metaServer;

        protected BaseRequestMobile(CloudApi cloudApi, string authToken, string metaServer) : base(cloudApi)
        {
            _token = authToken;
            _metaServer = metaServer;
        }

        protected override string RelationalUri => $"{_metaServer}?token={_token}&client_id=cloud-android";

        protected override ResponseBodyStream Transport(Stream stream)
        {
            return new ResponseBodyStream(stream);
        }
    }
}
