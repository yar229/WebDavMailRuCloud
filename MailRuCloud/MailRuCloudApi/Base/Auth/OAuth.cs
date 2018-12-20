using System;
using System.Net;
using System.Threading.Tasks;
using YaR.MailRuCloud.Api.Base.Requests;
using YaR.MailRuCloud.Api.Base.Requests.Types;
using YaR.MailRuCloud.Api.Base.Requests.WebBin;
using YaR.MailRuCloud.Api.Common;
using YaR.MailRuCloud.Api.Extensions;

namespace YaR.MailRuCloud.Api.Base.Auth
{
    internal class OAuth : IAuth
    {
        private static readonly log4net.ILog Logger = log4net.LogManager.GetLogger(typeof(OAuth));

        private readonly HttpCommonSettings _settings;
        private readonly IBasicCredentials _creds;

        private readonly AuthCodeRequiredDelegate _onAuthCodeRequired;

        public OAuth(HttpCommonSettings settings, IBasicCredentials creds, AuthCodeRequiredDelegate onAuthCodeRequired)
        {
            _settings = settings;
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
                value =>
                {
                    if (null == value)
                        return TimeSpan.MaxValue;
                    return value.ExpiresIn.Add(-TimeSpan.FromMinutes(5));
                });
                //value => TimeSpan.FromSeconds(20));
        }

        public bool IsAnonymous => _creds.IsAnonymous;
        public string Login => _creds.Login;
        public string Password => _creds.Password;
        public string AccessToken => _authToken.Value?.Token;
        public string DownloadToken => _authToken.Value?.Token;
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
            if (_creds.IsAnonymous)
                return null;

            var req = await new OAuthRequest(_settings, _creds).MakeRequestAsync();
            var res = req.ToAuthTokenResult();

            if (res.IsSecondStepRequired)
            {
                if (null == _onAuthCodeRequired)
                    throw new Exception("No 2Factor plugin found.");

                string code = _onAuthCodeRequired.Invoke(_creds.Login, true);
                if (string.IsNullOrWhiteSpace(code))
                    throw new Exception("Empty 2Factor code");

                var ssreq = await new OAuthSecondStepRequest(_settings, _creds.Login, res.TsaToken, code)
                    .MakeRequestAsync();

                res = ssreq.ToAuthTokenResult();
            }

            return res;
        }

        private async Task<AuthTokenResult> Refresh(string refreshToken)
        {
            var req = await new OAuthRefreshRequest(_settings, refreshToken).MakeRequestAsync();
            var res = req.ToAuthTokenResult(refreshToken);
            return res;
        }
    }
}