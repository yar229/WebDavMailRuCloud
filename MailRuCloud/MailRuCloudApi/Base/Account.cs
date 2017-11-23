using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using YaR.MailRuCloud.Api.Base.Requests.Mobile;
using YaR.MailRuCloud.Api.Base.Requests.Web;
using YaR.MailRuCloud.Api.Extensions;

namespace YaR.MailRuCloud.Api.Base
{
    /// <summary>
    /// MAIL.RU account info.
    /// </summary>
    public class Account
    {
        private static readonly log4net.ILog Logger = log4net.LogManager.GetLogger(typeof(Account));

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
            Credentials = new Credentials(login, password);

            WebRequest.DefaultWebProxy.Credentials = CredentialCache.DefaultCredentials;
            Proxy = WebRequest.DefaultWebProxy;

            var twoFaHandler1 = twoFaHandler;
            if (twoFaHandler1 != null)
                AuthCodeRequiredEvent += twoFaHandler1.Get;

            _bannedShards = new Cached<List<ShardInfo>>(() => new List <ShardInfo>(),
                TimeSpan.FromMinutes(2));

            _cachedShards = new Cached<Dictionary<ShardType, ShardInfo>>(() => new ShardInfoRequest(_cloudApi).MakeRequestAsync().Result.ToShardInfo(),
                TimeSpan.FromSeconds(ShardsExpiresInSec));

            DownloadToken = new Cached<string>(() => new DownloadTokenRequest(_cloudApi).MakeRequestAsync().Result.ToToken(),
                TimeSpan.FromSeconds(DownloadTokenExpiresSec));

            AuthToken = new Cached<string>(() =>
                {
                    Logger.Debug("AuthToken expired, refreshing.");
                    var token = new AuthTokenRequest(_cloudApi).MakeRequestAsync().Result.ToToken();
                    DownloadToken.Expire();
                    return token;
                },
                TimeSpan.FromSeconds(AuthTokenExpiresInSec));

            AuthTokenMobile = new Cached<string>(() =>
                {
                    Logger.Debug("AuthTokenMobile expired, refreshing.");
                    var token = new MobAuthRequest(_cloudApi).MakeRequestAsync().Result.ToToken();
                    return token;
                },
                TimeSpan.FromSeconds(AuthTokenMobileExpiresInSec));
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

        internal Credentials Credentials { get; }

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
            var loginResult = await new LoginRequest(_cloudApi, Credentials)
                .MakeRequestAsync();

            // 2FA
            if (!string.IsNullOrEmpty(loginResult.Csrf))
            {
                string authCode = OnAuthCodeRequired(Credentials.Login, false);
                await new SecondStepAuthRequest(_cloudApi, loginResult.Csrf, Credentials.Login, authCode)
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
        /// Token for authorization in mobile version
        /// </summary>
        public readonly Cached<string> AuthTokenMobile;
        private const int AuthTokenMobileExpiresInSec = 58 * 60;


        /// <summary>
        /// Token for downloading files
        /// </summary>
        public readonly Cached<string> DownloadToken;
        private const int DownloadTokenExpiresSec = 20 * 60;

        private readonly Cached<Dictionary<ShardType, ShardInfo>> _cachedShards;
        private readonly Cached<List<ShardInfo>> _bannedShards;
        private const int ShardsExpiresInSec = 30 * 60;


        public void BanShardInfo(ShardInfo banShard)
        {
            if (!_bannedShards.Value.Any(bsh => bsh.Type == banShard.Type && bsh.Url == banShard.Url))
            {
                Logger.Warn($"Shard {banShard.Url} temporarily banned");
                _bannedShards.Value.Add(banShard);
            }
        }


        /// <summary>
        /// Get shard info that to do post get request. Can be use for anonymous user.
        /// </summary>
        /// <param name="shardType">Shard type as numeric type.</param>
        /// <returns>Shard info.</returns>
        public async Task<ShardInfo> GetShardInfo(ShardType shardType)
        {
            bool refreshed = false;
            for (int i = 0; i < 10; i++)
            {
                Thread.Sleep(80 * i);
                var ishards = await Task.Run(() => _cachedShards.Value);
                var ishard = ishards[shardType];
                var banned = _bannedShards.Value;
                if (banned.All(bsh => bsh.Url != ishard.Url))
                {
                    if (refreshed) DownloadToken.Expire();
                    return ishard;
                }
                _cachedShards.Expire();
                refreshed = true;
            }

            Logger.Error("Cannot get working shard.");

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
