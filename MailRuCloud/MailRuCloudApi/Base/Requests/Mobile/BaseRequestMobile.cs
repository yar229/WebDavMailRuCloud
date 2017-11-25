using System.IO;

namespace YaR.MailRuCloud.Api.Base.Requests.Mobile
{
    abstract class BaseRequestMobile<T> : BaseRequest<ResponseBodyStream, T> where T : class 
    {
        protected readonly string Token;

        protected BaseRequestMobile(CloudApi cloudApi) : base(cloudApi)
        {
            Token = cloudApi.Account.AuthTokenMobile.Value;
        }

        protected override string RelationalUri
        {
            get
            {
                var meta = CloudApi.Account.MetaServer.Value;
                return $"{meta.Url}?token={Token}&client_id=cloud-android";
            }
        }

        protected override ResponseBodyStream Transport(Stream stream)
        {
            return new ResponseBodyStream(stream);
        }
    }
}
