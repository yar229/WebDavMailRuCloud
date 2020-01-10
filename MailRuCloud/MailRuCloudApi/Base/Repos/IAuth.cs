using System.Net;

namespace YaR.Clouds.Base.Repos
{
    public interface IAuth
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