using System.Net;
using YaR.Clouds.Base;
using YaR.Clouds.Base.Streams.Cache;
using YaR.Clouds.Common;

namespace YaR.Clouds
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
        public bool UseLocks { get; set; }
        
        public bool UseDeduplicate { get; set; }

        public DeduplicateRulesBag DeduplicateRules { get; set; }

    }



}