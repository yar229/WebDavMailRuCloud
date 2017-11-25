using System;
using System.Text;
using YaR.MailRuCloud.Api.Base.Requests.Types;

namespace YaR.MailRuCloud.Api.Base.Requests.Web
{
   class CreateFileRequest : BaseRequestJson<StatusResult>
    {
        private readonly string _fullPath;
        private readonly string _hash;
        private readonly long _size;
        private readonly ConflictResolver _conflictResolver;

        public CreateFileRequest(CloudApi cloudApi, string fullPath, string hash, long size, ConflictResolver? conflictResolver) : base(cloudApi)
        {
            _fullPath = fullPath;
            _hash = hash;
            _size = size;
            _conflictResolver = conflictResolver ?? ConflictResolver.Rename;
        }

        protected override string RelationalUri => "/api/v2/file/add";

        protected override byte[] CreateHttpContent()
        {
            string filePart = $"&hash={_hash}&size={_size}";
            string data = $"home={Uri.EscapeDataString(_fullPath)}&conflict={_conflictResolver}&api=2&token={CloudApi.Account.AuthToken}" + filePart;

            return Encoding.UTF8.GetBytes(data);
        }
    }
}
