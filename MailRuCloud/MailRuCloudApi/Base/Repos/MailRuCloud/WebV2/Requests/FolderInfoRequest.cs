using System;
using YaR.Clouds.Base.Requests;
using YaR.Clouds.Base.Requests.Types;

namespace YaR.Clouds.Base.Repos.MailRuCloud.WebV2.Requests
{
    internal class FolderInfoRequest : BaseRequestJson<FolderInfoResult>
    {
        private readonly string _path;
        private readonly bool _isWebLink;
        private readonly int _offset;
        private readonly int _limit;

        public FolderInfoRequest(HttpCommonSettings settings, IAuth auth, RemotePath path, int offset = 0, int limit = int.MaxValue) 
            : base(settings, auth)
        {
            _isWebLink = path.IsLink;

            if (path.IsLink)
            {
                string ustr = path.Link.Href.OriginalString;
                _path = "/" + ustr.Remove(0, ustr.IndexOf("/public/", StringComparison.Ordinal) + "/public/".Length);
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
                    ? $"/api/v2/folder?weblink={Uri.EscapeDataString(_path)}&offset={_offset}&limit={_limit}"
                    : $"/api/v2/folder?home={Uri.EscapeDataString(_path)}&offset={_offset}&limit={_limit}";

                if (!Auth.IsAnonymous)
                    uri += $"&token={Auth.AccessToken}";

                return uri;
            }
        }
    }
}
