using System;
using YaR.MailRuCloud.Api.Base.Requests.Types;

namespace YaR.MailRuCloud.Api.Base.Requests.Web
{
    class ItemInfoRequest : BaseRequestJson<FolderInfoResult>
    {
        private readonly string _path;
        private readonly bool _isWebLink;
        private readonly int _offset;
        private readonly int _limit;

        public ItemInfoRequest(RequestInit init, string path, bool isWebLink = false, int offset = 0, int limit = int.MaxValue) : base(init)
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
                    ? $"/api/v2/file?token={Init.Token}&weblink={Uri.EscapeDataString(_path)}&offset={_offset}&limit={_limit}"
                    : $"/api/v2/file?token={Init.Token}&home={Uri.EscapeDataString(_path)}&offset={_offset}&limit={_limit}";
                return uri;
            }
        }
    }
}