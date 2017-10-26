// ReSharper disable All

using System.Collections.Generic;

namespace YaR.MailRuCloud.Api.Base.Requests.Types
{

    public class ShardInfoResult
    {
        public string email { get; set; }
        public ShardInfoResultBody body { get; set; }
        public long time { get; set; }
        public int status { get; set; }
    }

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
    }

    public class ShardSection
    {
        public string count { get; set; }
        public string url { get; set; }
    }




}
