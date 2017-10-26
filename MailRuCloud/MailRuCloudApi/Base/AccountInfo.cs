namespace YaR.MailRuCloud.Api.Base
{
    public class AccountInfo
    {
        private long _fileSizeLimit;

        public long FileSizeLimit
        {
            get => _fileSizeLimit <= 0 ? long.MaxValue : _fileSizeLimit;
            set => _fileSizeLimit = value;
        }
    }
}


