using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using YaR.MailRuCloud.Api.Base.Requests;
using YaR.MailRuCloud.Api.Extensions;

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

            _cachedShards = new Cached<Dictionary<ShardType, ShardInfo>>(() => new ShardInfoRequest(_cloudApi).MakeRequestAsync().Result.ToShardInfo(),
                TimeSpan.FromSeconds(ShardsExpiresInSec));

            DownloadToken = new Cached<string>(() =>
                {
                    //TODO: not thread safe
                    var token = new DownloadTokenRequest(_cloudApi).MakeRequestAsync().Result.ToToken();
                    _cachedShards.Expire();
                    return token;
                },
                TimeSpan.FromSeconds(DownloadTokenExpiresSec));

            AuthToken = new Cached<string>(() =>
                {
                    //TODO: not thread safe
                    var token = new AuthTokenRequest(_cloudApi).MakeRequestAsync().Result.ToToken();
                    DownloadToken.Expire();
                    return token;
                },
                TimeSpan.FromSeconds(AuthTokenExpiresInSec));
        }

        /// <summary>
        /// Gets connection proxy.
        /// </summary>
        /// <value>Proxy settings.</value>
        public IWebProxy Proxy { get; }

        /// <summary>
        /// Gets account cookies.
        /// </summary>
        /// <value>Account cookies.</value>
        public CookieContainer Cookies => _cookies ?? (_cookies = new CookieContainer());

        /// <summary>
        /// Gets or sets login name.
        /// </summary>
        /// <value>Account email.</value>
        public string LoginName { get; }

        /// <summary>
        /// Gets or sets email password.
        /// </summary>
        /// <value>Password related with login.</value>
        private string Password { get; }

        public AccountInfo Info { get; private set; }

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

            Info = (await new AccountInfoRequest(_cloudApi)
                .MakeRequestAsync())
                .ToAccountInfo();

            return true;
        }

        /// <summary>
        /// Token for authorization
        /// </summary>
        public readonly Cached<string> AuthToken;
        private const int AuthTokenExpiresInSec = 23 * 60 * 60;

        /// <summary>
        /// Token for downloading files
        /// </summary>
        public readonly Cached<string> DownloadToken;
        private const int DownloadTokenExpiresSec = 20 * 60;

        private readonly Cached<Dictionary<ShardType, ShardInfo>> _cachedShards;
        private const int ShardsExpiresInSec = 2 * 60;

        /// <summary>
        /// Get shard info that to do post get request. Can be use for anonymous user.
        /// </summary>
        /// <param name="shardType">Shard type as numeric type.</param>
        /// <returns>Shard info.</returns>
        public async Task<ShardInfo> GetShardInfo(ShardType shardType)
        {
            var shards = await Task.Run(() => _cachedShards.Value);
            var shard = shards[shardType];
            return shard;
        }

        



        public delegate string AuthCodeRequiredDelegate(string login, bool isAutoRelogin);

        public event AuthCodeRequiredDelegate AuthCodeRequiredEvent;
        protected virtual string OnAuthCodeRequired(string login, bool isAutoRelogin)
        {
            return AuthCodeRequiredEvent?.Invoke(login, isAutoRelogin);
        }
    }
}
