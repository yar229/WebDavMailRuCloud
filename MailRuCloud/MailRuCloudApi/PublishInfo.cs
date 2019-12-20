using System;
using System.Collections.Generic;

namespace YaR.Clouds
{
    public class PublishInfo
    {
        public const string SharedFilePostfix = ".share.wdmrc";
        public const string PlaylistFilePostfix = ".m3u8";

        public List<PublishInfoItem> Items { get; set; }  = new List<PublishInfoItem>();
        public DateTime DateTime { get; set; } = DateTime.Now;
    }

    public class PublishInfoItem
    {
        public string Path { get; set; }
        public string Url { get; set; }
        public string PlaylistUrl { get; set; }
    }
}