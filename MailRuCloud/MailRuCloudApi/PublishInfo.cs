using System;

namespace YaR.MailRuCloud.Api
{
    public class PublishInfo
    {
        public const string SharedFilePostfix = ".share.wdmrc";

        public string Url { get; set; }
        public DateTime DateTime { get; set; }
    }
}