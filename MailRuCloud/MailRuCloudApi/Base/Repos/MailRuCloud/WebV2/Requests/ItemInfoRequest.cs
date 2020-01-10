using System;
using YaR.Clouds.Base.Requests;
using YaR.Clouds.Base.Requests.Types;

namespace YaR.Clouds.Base.Repos.MailRuCloud.WebV2.Requests
{
    class ItemInfoRequest : BaseRequestJson<FolderInfoResult>
    {
        private readonly string _path;
        private readonly bool _isWebLink;
        private readonly int _offset;
        private readonly int _limit;


        public ItemInfoRequest(HttpCommonSettings settings, IAuth auth, RemotePath path, int offset = 0, int limit = int.MaxValue) 
            : base(settings, auth)
        {
            _isWebLink = path.IsLink;

            if (path.IsLink)
            {
                string ustr = path.Link.Href.OriginalString;
                _path = "/" + ustr.Remove(0, ustr.IndexOf("/public/") + "/public/".Length);
            }
            else
                _path = path.Path;

            _offset = offset;
            _limit = limit;
        }

        protected override string RelationalUri
        {
            get
            {
                var uri = _isWebLink
                    ? $"/api/v2/file?token={Auth.AccessToken}&weblink={Uri.EscapeDataString(_path)}&offset={_offset}&limit={_limit}"
                    : $"/api/v2/file?token={Auth.AccessToken}&home={Uri.EscapeDataString(_path)}&offset={_offset}&limit={_limit}";
                return uri;
            }
        }
    }
}