using System.Net;
using YaR.MailRuCloud.Api.Base;
using YaR.MailRuCloud.Api.Common;

namespace YaR.MailRuCloud.Api
{
    public class CloudSettings
    {
	    public ITwoFaHandler TwoFaHandler { get; set; }
        public string UserAgent { get; set; }

        public Protocol Protocol { get; set; }

        public int CacheListingSec { get; set; } = 30;

	    public int ListDepth
	    {
		    get => CacheListingSec > 0 ? _listDepth : 1;
	        set => _listDepth = value;
	    }
		private int _listDepth = 1;

        public string SpecialCommandPrefix { get; set; } = ">>";
        public string AdditionalSpecialCommandPrefix { get; set; } = ">>";

        public SharedVideoResolution DefaultSharedVideoResolution { get; set; } = SharedVideoResolution.All;
        public IWebProxy Proxy { get; set; }
    }
}