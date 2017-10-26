using System;
using System.Text;
using YaR.MailRuCloud.Api.Base.Requests.Types;

namespace YaR.MailRuCloud.Api.Base.Requests
{
   class CreateFileRequest : BaseRequest<UnknownResult>
    {
        private readonly string _fullPath;
        private readonly string _hash;
        private readonly long _size;
        private readonly ResolveFileConflictMethod _onConflict;

        public CreateFileRequest(CloudApi cloudApi, string fullPath, string hash, long size, ResolveFileConflictMethod onConflict) : base(cloudApi)
        {
            _fullPath = fullPath;
            _hash = hash;
            _size = size;
            _onConflict = onConflict;
        }

        public override string RelationalUri => "/api/v2/file/add";

        protected override byte[] CreateHttpContent()
        {
            string filePart = $"&hash={_hash}&size={_size}";
            string data = $"home={Uri.EscapeDataString(_fullPath)}&conflict={GetConflictSolverParameter(_onConflict)}&api=2&token={CloudApi.Account.AuthToken}" + filePart;

            return Encoding.UTF8.GetBytes(data);
        }

        private string GetConflictSolverParameter(ResolveFileConflictMethod conflict = ResolveFileConflictMethod.Rewrite)
        {
            switch (conflict)
            {
                case ResolveFileConflictMethod.Rewrite:
                    return "rewrite";
                case ResolveFileConflictMethod.Rename:
                    return "rename";
                default: throw new NotImplementedException("File conflict method not implemented");
            }
        }
    }

    public enum ResolveFileConflictMethod
    {
        Rename,
        Rewrite
    }
}
