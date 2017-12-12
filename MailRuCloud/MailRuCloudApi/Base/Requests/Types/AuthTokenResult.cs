using System;

namespace YaR.MailRuCloud.Api.Base.Requests.Types
{
    public class AuthTokenResult
    {
        public bool IsSuccess { get; set; }
        public string Token { get; set; }
        public TimeSpan ExpiresIn { get; set; }
        public string RefreshToken { get; set; }
        public bool IsSecondStepRequired { get; set; }
        public string TsaToken { get; set; }
    }
}
