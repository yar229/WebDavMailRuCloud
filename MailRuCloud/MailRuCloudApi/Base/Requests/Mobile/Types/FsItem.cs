using System.Collections.Generic;

namespace YaR.MailRuCloud.Api.Base.Requests.Mobile.Types
{
    public class FsItem
    {
        public List<FsItem> Items { get; } = new List<FsItem>();
    }
}