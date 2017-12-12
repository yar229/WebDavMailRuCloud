using System;

namespace YaR.MailRuCloud.Api.Base.Requests.WebBin.Types
{
    class FsFile : FsItem
    {
        public string FullPath { get; }
        public ulong Size { get; }
        public DateTime ModifDate { get; }
        public byte[] Sha1 { get; }


        public FsFile(string fullPath, DateTime modifDate, byte[] sha1, ulong size)
        {
            FullPath = fullPath;
            ModifDate = modifDate;
            Sha1 = sha1;
            Size = size;
        }
    }
}