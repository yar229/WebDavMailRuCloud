namespace YaR.MailRuCloud.Api.Base.Requests.Types
{
    public class PublishResult
    {
        public string email { get; set; }
        public string body { get; set; }
        public long time { get; set; }
        public int status { get; set; }
    }
}