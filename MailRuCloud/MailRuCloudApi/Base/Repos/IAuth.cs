using System.Net;

namespace YaR.MailRuCloud.Api.Base.Repos
{
    internal interface IAuth
    {
        bool IsAnonymous { get; }

        string Login { get; }
        string Password { get; }
        string AccessToken { get; }
        string DownloadToken { get; }

        CookieContainer Cookies { get; }

        void ExpireDownloadToken();
    }
}