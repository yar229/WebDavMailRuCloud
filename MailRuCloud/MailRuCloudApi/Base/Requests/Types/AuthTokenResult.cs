// ReSharper disable All

namespace YaR.MailRuCloud.Api.Base.Requests.Types
{
    public class AuthTokenResultBody
    {
        public string token { get; set; }
    }

    public class AuthTokenResult
    {
        public string email { get; set; }
        public AuthTokenResultBody body { get; set; }
        public long time { get; set; }
        public int status { get; set; }
    }
}
