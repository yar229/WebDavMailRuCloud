using System.Net;

namespace YaR.MailRuCloud.Api.Base.Requests.Repo
{
    internal interface IAuth
    {
        string Login { get; }
        string Password { get; }
        string AccessToken { get; }
        string DownloadToken { get; }

        CookieContainer Cookies { get; }

        void ExpireDownloadToken();
    }
}