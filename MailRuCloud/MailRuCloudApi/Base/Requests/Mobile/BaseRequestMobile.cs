using System.IO;

namespace YaR.MailRuCloud.Api.Base.Requests.Mobile
{
    abstract class BaseRequestMobile<T> : BaseRequest<ResponseBodyStream, T> where T : class 
    {
        protected BaseRequestMobile(CloudApi cloudApi) : base(cloudApi)
        {
        }

        protected override ResponseBodyStream Transport(Stream stream)
        {
            return new ResponseBodyStream(stream);
        }
    }
}
