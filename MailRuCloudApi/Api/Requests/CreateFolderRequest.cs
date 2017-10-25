using System;
using System.Text;
using MailRuCloudApi.Api.Requests.Types;

namespace MailRuCloudApi.Api.Requests
{
   class CreateFolderRequest : BaseRequest<UnknownResult>
    {
        private readonly string _fullPath;

        public CreateFolderRequest(CloudApi cloudApi, string fullPath) : base(cloudApi)
        {
            _fullPath = fullPath;
        }

        public override string RelationalUri => "/api/v2/folder/add";

        protected override byte[] CreateHttpContent()
        {
            var data = $"home={Uri.EscapeDataString(_fullPath)}&conflict=rename&api={2}&token={CloudApi.Account.AuthToken}";
            return Encoding.UTF8.GetBytes(data);
        }
    }
}
