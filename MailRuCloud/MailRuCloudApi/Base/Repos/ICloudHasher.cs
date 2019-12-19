using System;

namespace YaR.Clouds.Base.Repos
{
    public interface ICloudHasher : IDisposable
    {
        void Append(byte[] buffer, int offset, int count);
        string HashString { get; }
        long Length { get; }
    }
}