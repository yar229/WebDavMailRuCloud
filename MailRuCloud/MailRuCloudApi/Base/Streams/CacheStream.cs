using System;
using System.Diagnostics;
using System.IO;

namespace YaR.Clouds.Base.Streams
{
    internal class CacheStream
    {
        private readonly File _file;
        private readonly Stream _sourceStream;
        private readonly object _cachesettings;

        public CacheStream(File file, Stream sourceStream, object cachesettings)
        {
            _file = file;
            _sourceStream = sourceStream;
            _cachesettings = cachesettings;
        }

        public bool Process()
        {
            _cache = GetCache();
            if (null == _cache)
                return false;

            _cache.FillFrom(_sourceStream);
            return true;
        }

        private DataCache _cache { get; set; }

        public Stream Stream => _cache?.OutStream ?? _sourceStream;


        private DataCache GetCache()
        {
            return new MemoryDataCache();
        }
    }

    internal abstract class DataCache
    {
        public abstract Stream OutStream { get; }

        public abstract void FillFrom(Stream sourceStream);
    }

    internal class MemoryDataCache : DataCache
    {
        private readonly MemoryStream _ms = new MemoryStream();


        public override Stream OutStream => _ms;

        public override void FillFrom(Stream sourceStream)
        {
            sourceStream.CopyTo(_ms);
            _ms.Seek(0, SeekOrigin.Begin);
        }
    }

    //internal class FileDataCache : DataCache
    //{
    //    public override Stream Stream { get; }
    //}

}