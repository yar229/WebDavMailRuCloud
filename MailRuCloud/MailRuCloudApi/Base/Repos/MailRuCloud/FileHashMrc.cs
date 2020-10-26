using System;
using YaR.Clouds.Extensions;

namespace YaR.Clouds.Base.Repos.MailRuCloud
{
    readonly struct FileHashMrc : IFileHash
    {
        public FileHashMrc(string value)
        {
            Hash = new Hash(FileHashType.Mrc, value);
        }

        public FileHashMrc(byte[] value) : this(value.ToHexString())
        {
        }

        public Hash Get(FileHashType htype)
        {
            if (htype != FileHashType.Mrc)
                throw new ArgumentException($"Mail.ru Cloud supportd only {FileHashType.Mrc} hash type");

            return Hash;
        }

        public Hash Hash { get; }

        public override string ToString()
        {
            return $"{FileHashType.Mrc}={Hash.Value}";
        }
    }
}
