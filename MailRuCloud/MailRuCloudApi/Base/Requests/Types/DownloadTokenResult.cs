namespace YaR.MailRuCloud.Api.Base.Requests.Types
{
    public class DownloadTokenResult
    {
        public string email { get; set; }
        public DownloadTokenBody body { get; set; }
        public long time { get; set; }
        public int status { get; set; }
    }
    public class DownloadTokenBody
    {
        public string token { get; set; }
    }
}