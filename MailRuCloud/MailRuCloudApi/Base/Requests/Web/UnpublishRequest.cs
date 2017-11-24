using System;
using System.Text;
using YaR.MailRuCloud.Api.Base.Requests.Types;

namespace YaR.MailRuCloud.Api.Base.Requests.Web
{
    class UnpublishRequest : BaseRequest<PublishResult>
    {
        private readonly string _publicLink;

        public UnpublishRequest(CloudApi cloudApi, string publicLink) : base(cloudApi)
        {
            _publicLink = publicLink;
        }

        protected override string RelationalUri => "/api/v2/file/unpublish";

        protected override byte[] CreateHttpContent()
        {
            var data = string.Format("weblink={0}&api={1}&token={2}&email={3}&x-email={3}", Uri.EscapeDataString(_publicLink),
                2, CloudApi.Account.AuthToken, CloudApi.Account.Credentials.Login);
            return Encoding.UTF8.GetBytes(data);
        }
    }
}