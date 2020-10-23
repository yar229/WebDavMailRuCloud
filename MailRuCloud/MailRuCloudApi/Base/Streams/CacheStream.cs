using System;
using System.IO;

namespace YaR.Clouds.Base.Streams
{
    internal class CacheStream : IDisposable
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

        private DataCache _cache;

        public Stream Stream => _cache?.OutStream ?? _sourceStream;


        private DataCache GetCache()
        {
            return new MemoryDataCache();
        }

        public void Dispose()
        {
            _cache?.Dispose();
        }
    }

    internal abstract class DataCache : IDisposable
    {
        public abstract Stream OutStream { get; }

        public abstract void FillFrom(Stream sourceStream);

        public abstract void Dispose();
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

        public override void Dispose()
        {
            _ms?.Dispose();
        }
    }

    //internal class FileDataCache : DataCache
    //{
    //    public override Stream Stream { get; }
    //}

}