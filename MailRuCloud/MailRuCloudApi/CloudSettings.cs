using log4net.Repository.Hierarchy;
using YaR.MailRuCloud.Api.Base;

namespace YaR.MailRuCloud.Api
{
    public class CloudSettings
    {
        public ITwoFaHandler TwoFaHandler { get; set; }
        public string UserAgent { get; set; }

        public Protocol Protocol { get; set; }

        public int CacheListingSec { get; set; } = 30;
	    
		public int ListDepth { get; set; }
	}
}