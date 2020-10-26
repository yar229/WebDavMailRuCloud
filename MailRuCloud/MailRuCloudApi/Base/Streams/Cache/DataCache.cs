using System;
using System.IO;

namespace YaR.Clouds.Base.Streams.Cache
{
    internal abstract class DataCache : IDisposable
    {
        public abstract string Name { get; }

        public abstract Stream OutStream { get; }

        public abstract void FillFrom(Stream sourceStream);

        public abstract void Dispose();
    }
}