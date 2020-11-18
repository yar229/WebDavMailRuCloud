namespace YaR.Clouds.Base
{
    public enum FileHashType
    {
        Mrc,

        YadSha256,
        YadMd5
    }

    public readonly struct Hash
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
}
