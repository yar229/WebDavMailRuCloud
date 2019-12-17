using System;
using System.Net;
using System.Security.Authentication;
using System.Threading.Tasks;
using YaR.MailRuCloud.Api.Base.Repos.MailRuCloud.WebV2.Requests;
using YaR.MailRuCloud.Api.Base.Requests;
using YaR.MailRuCloud.Api.Base.Requests.Types;
using YaR.MailRuCloud.Api.Common;

namespace YaR.MailRuCloud.Api.Base.Repos.MailRuCloud.WebV2
{
    class WebAuth : IAuth
    {
        private static readonly log4net.ILog Logger = log4net.LogManager.GetLogger(typeof(WebAuth));

        public CookieContainer Cookies { get; }

        private readonly HttpCommonSettings _settings;
        private readonly IBasicCredentials _creds;

        public WebAuth(HttpCommonSettings settings, IBasicCredentials creds, AuthCodeRequiredDelegate onAuthCodeRequired)
        {
            _settings = settings;
            _creds = creds;
            Cookies = new CookieContainer();

            var logged = MakeLogin(onAuthCodeRequired).Result;
            if (!logged)
                throw new AuthenticationException($"Cannot log in {creds.Login}");


            _authToken = new Cached<AuthTokenResult>(old =>
                {
                    Logger.Debug("AuthToken expired, refreshing.");
                    if (!creds.IsAnonymous)
                    {
                        var token = Auth().Result;
                        return token;
                    }
                    return null;
                },
                value => TimeSpan.FromSeconds(AuthTokenExpiresInSec));

            _cachedDownloadToken = new Cached<string>(old => new DownloadTokenRequest(_settings, this).MakeRequestAsync().Result.ToToken(),
                value => TimeSpan.FromSeconds(DownloadTokenExpiresSec));

        }

        public async Task<bool> MakeLogin(AuthCodeRequiredDelegate onAuthCodeRequired)
        {
            var loginResult = await new LoginRequest(_settings, this)
                .MakeRequestAsync();

            // 2FA
            if (!string.IsNullOrEmpty(loginResult.Csrf))
            {
                string authCode = onAuthCodeRequired(_creds.Login, false);
                await new SecondStepAuthRequest(_settings, loginResult.Csrf, authCode)
                    .MakeRequestAsync();
            }

            await new EnsureSdcCookieRequest(_settings, this)
                .MakeRequestAsync();

            return true;
        }

        public async Task<AuthTokenResult> Auth()
        {
            var req = await new AuthTokenRequest(_settings, this).MakeRequestAsync();
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


        public bool IsAnonymous => _creds.IsAnonymous;
        public string Login => _creds.Login;
        public string Password => _creds.Password;

        public string AccessToken => _authToken.Value?.Token;
        public string DownloadToken => _cachedDownloadToken.Value;

        public void ExpireDownloadToken()
        {
            _cachedDownloadToken.Expire();
        }
    }
}