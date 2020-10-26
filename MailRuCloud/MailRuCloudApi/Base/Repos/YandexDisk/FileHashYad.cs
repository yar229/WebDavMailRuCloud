using System;
using YaR.Clouds.Extensions;

namespace YaR.Clouds.Base.Repos.YandexDisk
{
    struct FileHashYad : IFileHash
    {
        public FileHashYad(byte[] hashSha256, byte[] hashMd5) : this()
        {
            HashSha256 = new Hash(FileHashType.YadSha256, hashSha256.ToHexString());
            HashMd5 = new Hash(FileHashType.YadMd5, hashMd5.ToHexString());
        }

        public Hash Get(FileHashType htype)
        {
            if (htype != FileHashType.YadSha256 && htype != FileHashType.YadMd5 )
                throw new ArgumentException($"Mail.ru Cloud supportd only {FileHashType.YadSha256} and {FileHashType.YadMd5} hash type");

            return Hash;
        }

        public Hash Hash => HashSha256;

        public Hash HashSha256 { get; private set; }
        public Hash HashMd5 { get; private set; }

        public override string ToString()
        {
            return $"{FileHashType.YadSha256}={HashSha256.Value}, {FileHashType.YadMd5}={HashMd5.Value}";
        }
    }
}
