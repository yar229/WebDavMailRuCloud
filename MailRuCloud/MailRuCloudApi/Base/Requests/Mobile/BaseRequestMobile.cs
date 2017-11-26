using System.IO;
using System.Net;

namespace YaR.MailRuCloud.Api.Base.Requests.Mobile
{
    abstract class BaseRequestMobile<T> : BaseRequest<ResponseBodyStream, T> where T : class 
    {
        private readonly string _metaServer;

        protected BaseRequestMobile(RequestInit init, string metaServer) : base(init)
        {
            _metaServer = metaServer;
        }

        protected override string RelationalUri => $"{_metaServer}?token={Init.Token}&client_id=cloud-android";

        protected override ResponseBodyStream Transport(Stream stream)
        {
            return new ResponseBodyStream(stream);
        }
    }
}
