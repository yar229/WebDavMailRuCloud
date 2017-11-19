using System;

namespace YaR.MailRuCloud.Api.Base
{
    public class HeaderFileContent
    {
        public string Name { get; set; }
        public long Size { get; set; }
        public CryptoKeyInfo PublicKey { get; set; }
        public DateTime CreationDate { get; set; }
    }
}