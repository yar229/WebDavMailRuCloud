using System.Net;
using YaR.MailRuCloud.Api.Base.Requests.Types;

namespace YaR.MailRuCloud.Api.Base.Requests
{
    public class RequestInit
    {
        private readonly Cached<AuthTokenResult> _auth;

        public RequestInit(IWebProxy proxy, CookieContainer cookies, Cached<AuthTokenResult> auth, string login)
        {
            _auth = auth;
            Proxy = proxy;
            Cookies = cookies;
            Login = login;
        }

        public IWebProxy Proxy { get; }
        public CookieContainer Cookies { get; }
        public string Token => _auth.Value.Token;
        public string Login { get; }
    }
}