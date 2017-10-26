using System;
using System.Text;
using YaR.MailRuCloud.Api.Base.Requests.Types;

namespace YaR.MailRuCloud.Api.Base.Requests
{

   class RemoveRequest : BaseRequest<UnknownResult>
    {
        private readonly string _fullPath;

        public RemoveRequest(CloudApi cloudApi, string fullPath) : base(cloudApi)
        {
            _fullPath = fullPath;
        }

        public override string RelationalUri
        {
            get
            {
                const string uri = "/api/v2/file/remove";
                return uri;
            }
        }

        protected override byte[] CreateHttpContent()
        {
            var data = string.Format("home={0}&api={1}&token={2}&email={3}&x-email={3}", Uri.EscapeDataString(_fullPath),
                2, CloudApi.Account.AuthToken, CloudApi.Account.LoginName);
            return Encoding.UTF8.GetBytes(data);
        }
    }
}
