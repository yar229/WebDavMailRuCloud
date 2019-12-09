using System.Collections.Generic;

namespace YaR.MailRuCloud.Api.Base.Repos.Mobile.Requests.Types
{
    public class FsItem
    {
        public List<FsItem> Items { get; } = new List<FsItem>();
    }
}