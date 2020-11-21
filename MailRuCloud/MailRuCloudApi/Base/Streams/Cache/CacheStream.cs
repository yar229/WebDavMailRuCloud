using System;
using System.IO;
using System.Text.RegularExpressions;

namespace YaR.Clouds.Base.Streams.Cache
{
    internal class CacheStream : IDisposable
    {
        private readonly File _file;
        private readonly Stream _sourceStream;
        private readonly DeduplicateRulesBag _deduplicateRules;

        public CacheStream(File file, Stream sourceStream, DeduplicateRulesBag deduplicateRules)
        {
            _file = file;
            _sourceStream = sourceStream;
            _deduplicateRules = deduplicateRules;
        }

        public bool Process()
        {
            _cache = GetCache();
            if (null == _cache)
                return false;

            _cache.FillFrom(_sourceStream);
            return true;
        }

        public string DataCacheName => _cache?.Name;
        private DataCache _cache;

        public Stream Stream => _cache?.OutStream ?? _sourceStream;


        private DataCache GetCache()
        {
            foreach (var rule in _deduplicateRules.Rules)
            {
                if (
                    (rule.MaxSize == 0 || rule.MaxSize > _file.Size) && 
                    _file.Size >= rule.MinSize &&
                    (string.IsNullOrEmpty(rule.Target) || Regex.Match(_file.FullPath, rule.Target).Success) )
                {
                    return rule.CacheType switch
                    {
                        CacheType.Memory => new MemoryDataCache(),
                        CacheType.Disk => new DiskDataCache(_deduplicateRules.DiskPath),
                        _ => throw new NotImplementedException($"DataCache not implemented for {rule.CacheType}")
                    };
                }
            }
            return null;
        }

        public void Dispose()
        {
            _cache?.Dispose();
        }
    }
}