// ReSharper disable All

namespace YaR.MailRuCloud.Api.Base.Requests.Types
{
    class MoveOrCopyResult
    {
        public string email { get; set; }
        public string body { get; set; }
        public long time { get; set; }
        public int status { get; set; }
    }
}
