using System.Collections.Generic;

namespace YaR.MailRuCloud.Api.Base.Repos.MailRuCloud.Mobile.Requests.Types
{
    public class FsItem
    {
        public List<FsItem> Items { get; } = new List<FsItem>();
    }
}