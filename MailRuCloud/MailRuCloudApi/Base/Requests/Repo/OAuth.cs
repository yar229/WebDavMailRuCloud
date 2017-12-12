using System;
using System.Net;
using System.Threading.Tasks;
using YaR.MailRuCloud.Api.Base.Requests.Types;
using YaR.MailRuCloud.Api.Base.Requests.WebBin;
using YaR.MailRuCloud.Api.Extensions;

namespace YaR.MailRuCloud.Api.Base.Requests.Repo
{
    internal class OAuth : IAuth
    {
        private static readonly log4net.ILog Logger = log4net.LogManager.GetLogger(typeof(OAuth));

        private readonly IWebProxy _proxy;
        private readonly IBasicCredentials _creds;
        private readonly AuthCodeRequiredDelegate _onAuthCodeRequired;
        private const string ClientId = "cloud-android";

        public OAuth(IWebProxy proxy, IBasicCredentials creds, AuthCodeRequiredDelegate onAuthCodeRequired)
        {
            _proxy = proxy;
            _creds = creds;
            _onAuthCodeRequired = onAuthCodeRequired;
            Cookies = new CookieContainer();

            _authToken = new Cached<AuthTokenResult>(old =>
                {
                    Logger.Debug(null == old ? "OAuth: authorizing." : "OAuth: AuthToken expired, refreshing.");

                    var token = null == old || string.IsNullOrEmpty(old.RefreshToken)
                        ? Auth().Result
                        : Refresh(old.RefreshToken).Result;

                    return token;
                },
                value => value.ExpiresIn.Add(-TimeSpan.FromMinutes(5)));
                //value => TimeSpan.FromSeconds(20));
        }

        public string Login => _creds.Login;
        public string Password => _creds.Password;
        public string AccessToken => _authToken.Value.Token;
        public string DownloadToken => _authToken.Value.Token;
        public CookieContainer Cookies { get; }

        public void ExpireDownloadToken()
        {
        }

        /// <summary>
        /// Token for authorization in mobile version
        /// </summary>
        private readonly Cached<AuthTokenResult> _authToken;

        private async Task<AuthTokenResult> Auth()
        {
            var req = await new OAuthRequest(_proxy, _creds, ClientId).MakeRequestAsync();
            var res = req.ToAuthTokenResult();

            if (res.IsSecondStepRequired)
            {
                if (null == _onAuthCodeRequired)
                    throw new Exception("No 2Factor plugin found.");

                string code = _onAuthCodeRequired.Invoke(_creds.Login, true);
                if (string.IsNullOrWhiteSpace(code))
                    throw new Exception("Empty 2Factor code");

                var ssreq = await new OAuthSecondStepRequest(_proxy, ClientId, _creds.Login, res.TsaToken, code)
                    .MakeRequestAsync();

                res = ssreq.ToAuthTokenResult();
            }

            return res;
        }

        private async Task<AuthTokenResult> Refresh(string refreshToken)
        {
            var req = await new OAuthRefreshRequest(_proxy, ClientId, refreshToken).MakeRequestAsync();
            var res = req.ToAuthTokenResult(refreshToken);
            return res;
        }
    }
}