using System;
using System.Collections.Generic;

namespace YaR.Clouds
{
    public class PublishInfo
    {
        public const string SharedFilePostfix = ".share.wdmrc";
        public const string PlaylistFilePostfix = ".m3u8";

        public List<PublishInfoItem> Items { get; }  = new();
        public DateTime DateTime { get; set; } = DateTime.Now;
    }

    public class PublishInfoItem
    {
        public string Path { get; set; }
        public List<Uri> Urls { get; set; }
        public string PlaylistUrl { get; set; }
    }
}