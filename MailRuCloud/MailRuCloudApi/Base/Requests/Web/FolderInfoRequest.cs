using System;
using YaR.MailRuCloud.Api.Base.Requests.Types;

namespace YaR.MailRuCloud.Api.Base.Requests.Web
{
    class FolderInfoRequest : BaseRequestJson<FolderInfoResult>
    {
        private readonly string _path;
        private readonly bool _isWebLink;
        private readonly int _offset;
        private readonly int _limit;

        public FolderInfoRequest(CloudApi cloudApi, string path, bool isWebLink = false, int offset = 0, int limit = int.MaxValue) : base(cloudApi)
        {
            _path = path;
            _isWebLink = isWebLink;
            _offset = offset;
            _limit = limit;
        }

        protected override string RelationalUri
        {
            get
            {
                var uri = _isWebLink
                    ? $"/api/v2/folder?token={CloudApi.Account.AuthToken}&weblink={Uri.EscapeDataString(_path)}&offset={_offset}&limit={_limit}"
                    : $"/api/v2/folder?token={CloudApi.Account.AuthToken}&home={Uri.EscapeDataString(_path)}&offset={_offset}&limit={_limit}";
                return uri;
            }
        }
    }
}
