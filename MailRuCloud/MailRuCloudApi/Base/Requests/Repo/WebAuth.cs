using System;
using System.Net;
using System.Security.Authentication;
using System.Threading.Tasks;
using YaR.MailRuCloud.Api.Base.Requests.Types;
using YaR.MailRuCloud.Api.Base.Requests.WebV2;
using YaR.MailRuCloud.Api.Extensions;

namespace YaR.MailRuCloud.Api.Base.Requests.Repo
{
    class WebAuth : IAuth
    {
        public CookieContainer Cookies { get; }

        private readonly IWebProxy _proxy;
        private readonly IBasicCredentials _creds;
        private static readonly log4net.ILog Logger = log4net.LogManager.GetLogger(typeof(WebAuth));

        public WebAuth(IWebProxy proxy, IBasicCredentials creds, AuthCodeRequiredDelegate onAuthCodeRequired)
        {
            _proxy = proxy;
            _creds = creds;
            Cookies = new CookieContainer();

            var logged = MakeLogin(onAuthCodeRequired).Result;
            if (!logged)
                throw new AuthenticationException($"Cannot log in {creds.Login}");

            _authToken = new Cached<AuthTokenResult>(() =>
                {
                    Logger.Debug("AuthToken expired, refreshing.");
                    var token = Auth().Result;
                    _cachedDownloadToken.Expire();
                    return token;
                },
                TimeSpan.FromSeconds(AuthTokenExpiresInSec));

            _cachedDownloadToken = new Cached<string>(() => new DownloadTokenRequest(_proxy, this).MakeRequestAsync().Result.ToToken(),
                TimeSpan.FromSeconds(DownloadTokenExpiresSec));
        }

        public async Task<bool> MakeLogin(AuthCodeRequiredDelegate onAuthCodeRequired)
        {
            var loginResult = await new LoginRequest(_proxy, this)
                .MakeRequestAsync();

            // 2FA
            if (!string.IsNullOrEmpty(loginResult.Csrf))
            {
                string authCode = onAuthCodeRequired(_creds.Login, false);
                await new SecondStepAuthRequest(_proxy, loginResult.Csrf, authCode)
                    .MakeRequestAsync();
            }

            await new EnsureSdcCookieRequest(_proxy, this)
                .MakeRequestAsync();

            return true;
        }

        public async Task<AuthTokenResult> Auth()
        {
            var req = await new AuthTokenRequest(_proxy, this).MakeRequestAsync();
            var res = req.ToAuthTokenResult();
            return res;
        }

        /// <summary>
        /// Token for authorization
        /// </summary>
        private readonly Cached<AuthTokenResult> _authToken;
        private const int AuthTokenExpiresInSec = 23 * 60 * 60;

        /// <summary>
        /// Token for downloading files
        /// </summary>
        private readonly Cached<string> _cachedDownloadToken;
        private const int DownloadTokenExpiresSec = 20 * 60;



        public string Login => _creds.Login;
        public string Password => _creds.Password;

        public string AccessToken => _authToken.Value.Token;
        public string DownloadToken => _cachedDownloadToken.Value;

        public void ExpireDownloadToken()
        {
            _cachedDownloadToken.Expire();
        }
    }
}