using System;
using System.Collections.Generic;
using System.Net;
using System.Security.Authentication;
using System.Threading.Tasks;
using YaR.MailRuCloud.Api.Base.Repos.YadWeb.Requests;
using YaR.MailRuCloud.Api.Base.Requests;

namespace YaR.MailRuCloud.Api.Base.Repos.YadWeb
{
    class YadWebAuth : IAuth
    {
        public YadWebAuth(HttpCommonSettings settings, IBasicCredentials creds)
        {
            _settings = settings;
            _creds = creds;
            Cookies = new CookieContainer();

            var z = MakeLogin().Result;
        }

        private readonly IBasicCredentials _creds;
        private readonly HttpCommonSettings _settings;

        public async Task<bool> MakeLogin()
        {
            var preAuthResult = await new YadPreAuthRequest(_settings, this)
                .MakeRequestAsync();
            Uuid = preAuthResult.ProcessUUID;

            var loginAuth = await new YadAuthLoginRequest(_settings, this, preAuthResult.Csrf, preAuthResult.ProcessUUID)
                    .MakeRequestAsync();
            if (loginAuth.HasError)
                throw new AuthenticationException($"{nameof(YadAuthLoginRequest)} error");

            var passwdAuth = await new YadAuthPasswordRequest(_settings, this, preAuthResult.Csrf, loginAuth.TrackId)
                .MakeRequestAsync();
            if (passwdAuth.HasError)
                throw new AuthenticationException($"{nameof(YadAuthPasswordRequest)} error");


            var accsAuth = await new YadAuthAccountsRequest(_settings, this, preAuthResult.Csrf)
                .MakeRequestAsync();
            if (accsAuth.HasError)
                throw new AuthenticationException($"{nameof(YadAuthAccountsRequest)} error");

            var skReq = await new YadAuthDiskSkRequest(_settings, this)
                .MakeRequestAsync();
            if (skReq.HasError)
                throw new AuthenticationException($"{nameof(YadAuthDiskSkRequest)} error");
            DiskSk = skReq.DiskSk;

            //Csrf = preAuthResult.Csrf;

            return true;
        }

        public string Login => _creds.Login;
        public string Password => _creds.Password;
        public string DiskSk { get; set; }
        public string Uuid { get; set; }
        //public string Csrf { get; set; }
        


        public bool IsAnonymous { get; }
        public string AccessToken { get; }
        public string DownloadToken { get; }
        public CookieContainer Cookies { get; }
        public void ExpireDownloadToken()
        {
            throw new NotImplementedException();
        }
    }
}
