using System;
using System.Text;
using YaR.MailRuCloud.Api.Base.Requests.Types;

namespace YaR.MailRuCloud.Api.Base.Requests.Web
{
    
    class RenameRequest : BaseRequestJson<RenameResult>
    {
        private readonly string _fullPath;
        private readonly string _newName;

        public RenameRequest(CloudApi cloudApi, string fullPath, string newName) : base(cloudApi)
        {
            _fullPath = fullPath;
            _newName = newName;
        }

        protected override string RelationalUri => "/api/v2/file/rename";

        protected override byte[] CreateHttpContent()
        {
            var data = string.Format("home={0}&api={1}&token={2}&email={3}&x-email={3}&conflict=rename&name={4}", Uri.EscapeDataString(_fullPath),
                2, CloudApi.Account.AuthToken, CloudApi.Account.Credentials.Login, Uri.EscapeDataString(_newName));
            return Encoding.UTF8.GetBytes(data);
        }
    }
}
