// ReSharper disable All

namespace MailRuCloudApi.Api.Requests.Types
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
