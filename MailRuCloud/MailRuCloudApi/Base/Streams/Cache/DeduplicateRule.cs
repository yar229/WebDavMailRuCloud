namespace YaR.Clouds.Base.Streams.Cache
{
    public struct DeduplicateRule
    {
        public CacheType CacheType { get; set; }
        public string Target { get; set; }
        public ulong MinSize { get; set; }
        public ulong MaxSize { get; set; }
    }
}