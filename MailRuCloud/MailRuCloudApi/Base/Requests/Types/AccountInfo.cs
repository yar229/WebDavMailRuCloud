namespace YaR.MailRuCloud.Api.Base.Requests.Types
{
    public class AccountInfoResult
    {
        private long _fileSizeLimit;

        public long FileSizeLimit
        {
            get => _fileSizeLimit <= 0 ? long.MaxValue : _fileSizeLimit;
            set => _fileSizeLimit = value;
        }

        public DiskUsage DiskUsage { get; set; }
    }
}


