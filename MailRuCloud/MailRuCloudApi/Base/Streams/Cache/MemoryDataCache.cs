using System.IO;

namespace YaR.Clouds.Base.Streams.Cache
{
    internal class MemoryDataCache : DataCache
    {
        private readonly MemoryStream _ms = new MemoryStream();

        public override string Name => nameof(MemoryDataCache);

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
}