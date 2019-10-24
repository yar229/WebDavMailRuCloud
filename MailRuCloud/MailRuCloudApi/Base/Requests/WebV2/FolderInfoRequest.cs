using System;
using YaR.MailRuCloud.Api.Base.Auth;
using YaR.MailRuCloud.Api.Base.Requests.Types;

namespace YaR.MailRuCloud.Api.Base.Requests.WebV2
{
    internal class FolderInfoRequest : BaseRequestJson<FolderInfoResult>
    {
        private readonly string _path;
        private readonly bool _isWebLink;
        private readonly int _offset;
        private readonly int _limit;

        public FolderInfoRequest(HttpCommonSettings settings, IAuth auth, string path, bool isWebLink = false, int offset = 0, int limit = int.MaxValue) 
            : base(settings, auth)
        {
            _path = path;
            _isWebLink = isWebLink;
            _offset = offset;
            _limit = limit;
        }

        //protected override string RelationalUri
        //{
        //    get
        //    {
        //        var uri = _isWebLink
        //            ? $"/api/v2/folder?token={Auth.AccessToken}&weblink={Uri.EscapeDataString(_path)}&offset={_offset}&limit={_limit}"
        //            : $"/api/v2/folder?token={Auth.AccessToken}&home={Uri.EscapeDataString(_path)}&offset={_offset}&limit={_limit}";
        //        return uri;
        //    }
        //}

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


        //protected override string RelationalUri
        //{
        //    get
        //    {
        //        var url = $"/api/v2/folder?offset={_offset}&limit={_limit}";
        //        if (!Auth.IsAnonymous)
        //            url += $"access_token={Auth.AccessToken}";

        //        return url;
        //        // $"/api/m1/folder?access_token={Auth.AccessToken}&offset={_offset}&limit={_limit}";
        //    }
        //}
    }
}
