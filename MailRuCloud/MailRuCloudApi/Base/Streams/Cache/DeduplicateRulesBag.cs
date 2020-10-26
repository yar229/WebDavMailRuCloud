using System.Collections.Generic;

namespace YaR.Clouds.Base.Streams.Cache
{
    public class DeduplicateRulesBag
    {
        public List<DeduplicateRule> Rules { get; set; }

        public string DiskPath { get; set; }
    }
}