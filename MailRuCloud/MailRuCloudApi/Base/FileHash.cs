using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using YaR.Clouds.Extensions;

namespace YaR.Clouds.Base
{
    public enum FileHashType
    {
        Mrc,

        YadSha256,
        YadMd5
    }

    public struct Hash
    {
        public Hash(FileHashType htype, string hash)
        {
            HashType = htype;
            Value = hash;
        }

        public FileHashType HashType { get; }
        public string Value { get; }
    }

    public interface IFileHash
    {
        Hash Get(FileHashType htype);
        Hash Hash { get; }
    }

    struct FileHashMrc : IFileHash
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
