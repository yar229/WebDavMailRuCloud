using System.Collections.Generic;
using YaR.MailRuCloud.Api.Base.Auth;

namespace YaR.MailRuCloud.Api.Base.Requests.WebV2
{
    class ShardInfoRequest : BaseRequestJson<ShardInfoRequest.Result>
    {
        public ShardInfoRequest(HttpCommonSettings settings, IAuth auth) 
            : base(settings, auth)
        {
        }

        protected override string RelationalUri
        {
            get
            {
                //var uri = string.Format("{0}/api/v2/dispatcher?api=2", ConstSettings.CloudDomain);
                //if (!string.IsNullOrEmpty(Auth.AccessToken))
                //    uri += $"&token={Auth.AccessToken}";
                //return uri;

                var uri = $"{ConstSettings.CloudDomain}/api/v2/dispatcher?client_id={Settings.ClientId}";
                if (!Auth.IsAnonymous)
                    uri += $"&access_token={Auth.AccessToken}";
                else
                {
                    uri += "&email=anonym&x-email=anonym";
                }
                return uri;

            }
        }


        public class Result
        {
            public string email { get; set; }
            public ShardInfoResultBody body { get; set; }
            public long time { get; set; }
            public int status { get; set; }

            public class ShardInfoResultBody
            {
                public List<ShardSection> video { get; set; }
                public List<ShardSection> view_direct { get; set; }
                public List<ShardSection> weblink_view { get; set; }
                public List<ShardSection> weblink_video { get; set; }
                public List<ShardSection> weblink_get { get; set; }
                public List<ShardSection> weblink_thumbnails { get; set; }
                public List<ShardSection> auth { get; set; }
                public List<ShardSection> view { get; set; }
                public List<ShardSection> get { get; set; }
                public List<ShardSection> upload { get; set; }
                public List<ShardSection> thumbnails { get; set; }

                public class ShardSection
                {
                    public string count { get; set; }
                    public string url { get; set; }
                }
            }
        }
    }
}
