using System;
using System.Net;
using System.Security.Authentication;
using System.Threading.Tasks;
using YaR.MailRuCloud.Api.Base.Requests;

namespace YaR.MailRuCloud.Api.Base
{
    /// <summary>
    /// MAIL.RU account info.
    /// </summary>
    public class Account
    {
        private readonly CloudApi _cloudApi;

        /// <summary>
        /// Default cookies.
        /// </summary>
        private CookieContainer _cookies;

        //private readonly AuthCodeWindow _authCodeHandler = new AuthCodeWindow();

        /// <summary>
        /// Initializes a new instance of the <see cref="Account" /> class.
        /// </summary>
        /// <param name="cloudApi"></param>
        /// <param name="login">Login name as email.</param>
        /// <param name="password">Password related with this login</param>
        /// <param name="twoFaHandler"></param>
        public Account(CloudApi cloudApi, string login, string password, ITwoFaHandler twoFaHandler)
        {
            _cloudApi = cloudApi;
            LoginName = login;
            Password = password;

            WebRequest.DefaultWebProxy.Credentials = CredentialCache.DefaultCredentials;
            Proxy = WebRequest.DefaultWebProxy;

            var twoFaHandler1 = twoFaHandler;
            if (twoFaHandler1 != null)
                AuthCodeRequiredEvent += twoFaHandler1.Get;
        }

        /// <summary>
        /// Gets or sets connection proxy.
        /// </summary>
        /// <value>Proxy settings.</value>
        public IWebProxy Proxy { get; set; }

        /// <summary>
        /// Gets authorization token.
        /// </summary>
        /// <value>Access token.</value>
        public string AuthToken { get; private set; }

        /// <summary>
        /// Gets account cookies.
        /// </summary>
        /// <value>Account cookies.</value>
        public CookieContainer Cookies => _cookies ?? (_cookies = new CookieContainer());

        /// <summary>
        /// Gets or sets login name.
        /// </summary>
        /// <value>Account email.</value>
        public string LoginName { get; set; }

        /// <summary>
        /// Gets or sets email password.
        /// </summary>
        /// <value>Password related with login.</value>
        public string Password { get; set; }

        public AccountInfo Info { get; set; }

        /// <summary>
        /// Authorize on MAIL.RU server.
        /// </summary>
        /// <returns>True or false result operation.</returns>
        public bool Login()
        {
            return LoginAsync().Result;
        }

        /// <summary>
        /// Async call to authorize on MAIL.RU server.
        /// </summary>
        /// <returns>True or false result operation.</returns>
        public async Task<bool> LoginAsync()
        {
            if (string.IsNullOrEmpty(LoginName))
            {
                throw new ArgumentException("LoginName is null or empty.");
            }

            if (string.IsNullOrEmpty(Password))
            {
                throw new ArgumentException("Password is null or empty.");
            }

            var loginResult = await new LoginRequest(_cloudApi, LoginName, Password)
                .MakeRequestAsync();

            // 2FA
            if (!string.IsNullOrEmpty(loginResult.Csrf))
            {
                string authCode = OnAuthCodeRequired(LoginName, false);
                await new SecondStepAuthRequest(_cloudApi, loginResult.Csrf, LoginName, authCode)
                    .MakeRequestAsync();
            }

            await new EnsureSdcCookieRequest(_cloudApi)
                .MakeRequestAsync();

            AuthToken = new AuthTokenRequest(_cloudApi)
                .MakeRequestAsync()
                .ThrowIf(data => string.IsNullOrEmpty(data.body?.token), new AuthenticationException("Empty auth token"))
                .body.token;

            Info = new AccountInfo
            {
                FileSizeLimit = new AccountInfoRequest(_cloudApi).MakeRequestAsync().Result.body.cloud.file_size_limit
            };

            TokenExpiresAt = DateTime.Now.AddHours(TokenExpiresInSec);

            return true;
        }

        public DateTime TokenExpiresAt { get; private set; }
        private const int TokenExpiresInSec = 23 * 60 * 60;


        public string DownloadToken
        {
            get
            {
                if (string.IsNullOrEmpty(_downloadToken) || (DateTime.Now - _downloadTokenDate).TotalSeconds > DownloadTokenExpiresSec)
                {
                    _downloadTokenDate = DateTime.Now;
                    var dtres = new DownloadTokenRequest(_cloudApi).MakeRequestAsync().Result;
                    _downloadToken = dtres.body.token;
                }
                return _downloadToken;
            }
        }
        private string _downloadToken;
        private DateTime _downloadTokenDate = DateTime.MinValue;
        private const int DownloadTokenExpiresSec = 2 * 60 * 60;

        /// <summary>
        /// Need to add this function for all calls.
        /// </summary>
        internal void CheckAuth()
        {
            if (LoginName == null || Password == null)
                throw new AuthenticationException("Login or password is empty.");

            if (string.IsNullOrEmpty(AuthToken))
                if (!Login())
                    throw new AuthenticationException("Auth token has't been retrieved.");
        }


        public delegate string AuthCodeRequiredDelegate(string login, bool isAutoRelogin);

        public event AuthCodeRequiredDelegate AuthCodeRequiredEvent;
        protected virtual string OnAuthCodeRequired(string login, bool isAutoRelogin)
        {
            return AuthCodeRequiredEvent?.Invoke(login, isAutoRelogin);
        }
    }

    public static class Extensions
    {
        public static T ThrowIf<T>(this Task<T> data, Func<T, bool> func, Exception ex)
        {
            var res = data.Result;
            if (func(res)) throw ex;
            return res;
        }
    }
}
