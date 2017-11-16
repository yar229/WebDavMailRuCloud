namespace YaR.MailRuCloud.Api.Base
{
    public class CryptInfo
    {
        public const uint HeaderSize = 1024;
        public uint AlignBytes { get; set; }
        public byte[] PublicKey { get; set; }
    }
}