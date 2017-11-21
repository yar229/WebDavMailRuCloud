using System;
using System.Text;
using YaR.MailRuCloud.Api.Base.Requests.Types;

namespace YaR.MailRuCloud.Api.Base.Requests
{

   class RemoveRequest : BaseRequest<RemoveResult>
    {
        private readonly string _fullPath;

        public RemoveRequest(CloudApi cloudApi, string fullPath) : base(cloudApi)
        {
            _fullPath = fullPath;
        }

        protected override string RelationalUri => "/api/v2/file/remove";

        protected override byte[] CreateHttpContent()
        {
            var data = string.Format("home={0}&api={1}&token={2}&email={3}&x-email={3}", Uri.EscapeDataString(_fullPath),
                2, CloudApi.Account.AuthToken, CloudApi.Account.Credentials.Login);
            return Encoding.UTF8.GetBytes(data);
        }
    }
}
