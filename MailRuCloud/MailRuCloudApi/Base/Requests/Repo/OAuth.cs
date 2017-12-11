using System;
using System.Net;
using System.Threading.Tasks;
using YaR.MailRuCloud.Api.Base.Requests.Mobile;
using YaR.MailRuCloud.Api.Base.Requests.Types;
using YaR.MailRuCloud.Api.Extensions;

namespace YaR.MailRuCloud.Api.Base.Requests.Repo
{
    internal class OAuth : IAuth
    {
        private static readonly log4net.ILog Logger = log4net.LogManager.GetLogger(typeof(OAuth));

        private readonly IWebProxy _proxy;
        private readonly IBasicCredentials _creds;

        public OAuth(IWebProxy proxy, IBasicCredentials creds, AuthCodeRequiredDelegate onAuthCodeRequired)
        {
            _proxy = proxy;
            _creds = creds;
            Cookies = new CookieContainer();

            _authTokenMobile = new Cached<AuthTokenResult>(() =>
                {
                    Logger.Debug("AuthTokenMobile expired, refreshing.");
                    var token = Auth().Result;
                    return token;
                },
                TimeSpan.FromSeconds(AuthTokenMobileExpiresInSec));
        }

        public string Login => _creds.Login;
        public string Password => _creds.Password;
        public string AccessToken => _authTokenMobile.Value.Token;
        public string DownloadToken => _authTokenMobile.Value.Token;
        public CookieContainer Cookies { get; }


        public void ExpireDownloadToken()
        {
        }


        /// <summary>
        /// Token for authorization in mobile version
        /// </summary>
        private readonly Cached<AuthTokenResult> _authTokenMobile;
        private const int AuthTokenMobileExpiresInSec = 58 * 60;


        public async Task<AuthTokenResult> Auth()
        {
            var req = await new MobAuthRequest(_proxy, _creds).MakeRequestAsync();
            var res = req.ToAuthTokenResult();
            return res;
        }

    }
}