using System;
using System.IO;

namespace YaR.Clouds.Base.Streams.Cache
{
    internal class DiskDataCache : DataCache
    {
        private readonly string _filename;

        public override string Name => nameof(DiskDataCache);

        public DiskDataCache(string basePath)
        {
            _filename = Path.Combine(basePath, Guid.NewGuid().ToString());
        }

        public override Stream OutStream => _outStream ??= new FileStream(_filename, FileMode.Open, FileAccess.Read);
        private Stream _outStream;

        public override void FillFrom(Stream sourceStream)
        {
            using (var fs = new FileStream(_filename, FileMode.Create, FileAccess.Write))
            {
                sourceStream.CopyTo(fs);
            }
        }

        public override void Dispose()
        {
            _outStream?.Dispose();

            if (System.IO.File.Exists(_filename))
                System.IO.File.Delete(_filename);
        }
    }
}