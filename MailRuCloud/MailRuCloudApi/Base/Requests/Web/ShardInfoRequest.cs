using System.Collections.Generic;

namespace YaR.MailRuCloud.Api.Base.Requests.Web
{
    class ShardInfoRequest : BaseRequestJson<ShardInfoRequest.Result>
    {
        public ShardInfoRequest(RequestInit init) : base(init)
        {
        }

        protected override string RelationalUri
        {
            get
            {
                var uri = string.Format("{0}/api/v2/dispatcher?api=2", ConstSettings.CloudDomain);
                if (!string.IsNullOrEmpty(Init.Token))
                    uri += $"&token={Init.Token}";
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
