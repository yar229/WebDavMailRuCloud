using System;

namespace YaR.MailRuCloud.Api.Links.Dto
{
    public class ItemLink
    {
        public string Href { get; set; }
        public string MapTo { get; set; }
        public string Name { get; set; }
        public bool IsFile { get; set; }
        public long Size { get; set; }
        public DateTime? CreationDate { get; set; }
    }
}