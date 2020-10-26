using System;
using System.IO;

namespace YaR.Clouds.Base.Repos
{
    public interface ICloudHasher : IDisposable
    {
        string Name { get; }

        void Append(byte[] buffer, int offset, int count);
        void Append(Stream stream);

        IFileHash Hash { get; }
        long Length { get; }
    }
}