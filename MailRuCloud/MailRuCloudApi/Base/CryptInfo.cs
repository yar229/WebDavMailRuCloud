namespace YaR.Clouds.Base
{
    public class CryptInfo
    {
        //public const uint HeaderSize = 1024;
        public uint AlignBytes { get; set; }
        public CryptoKeyInfo PublicKey { get; set; }
    }

    public class CryptoKeyInfo
    {
        public byte[] IV { get; set; }
        public byte[] Salt { get; set; }

    }
}