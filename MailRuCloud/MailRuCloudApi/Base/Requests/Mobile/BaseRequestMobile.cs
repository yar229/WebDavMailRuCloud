using System.IO;

namespace YaR.MailRuCloud.Api.Base.Requests.Mobile
{
    abstract class BaseRequestMobile<T> : BaseRequest<ResponseBodyStream, T> where T : class 
    {
        private readonly string _token;

        protected BaseRequestMobile(CloudApi cloudApi) : base(cloudApi)
        {
            _token = cloudApi.Account.AuthTokenMobile.Value;
        }

        protected override string RelationalUri
        {
            get
            {
                var meta = CloudApi.Account.MetaServer.Value;
                return $"{meta.Url}?token={_token}&client_id=cloud-android";
            }
        }

        protected override ResponseBodyStream Transport(Stream stream)
        {
            return new ResponseBodyStream(stream);
        }
    }
}
