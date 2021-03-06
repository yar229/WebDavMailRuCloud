﻿using System.Net;
using System.Threading.Tasks;
using YaR.Clouds.Base.Repos;
using YaR.Clouds.Base.Requests.Types;

namespace YaR.Clouds.Base
{
    //TODO: refact, maybe we don't need this class
    //TODO: refact, Requestrepo - wrong place?
    public class Account
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Account" /> class.
        /// </summary>
        public Account(CloudSettings settings, Credentials credentials)
        {
            Credentials = credentials;

            WebRequest.DefaultWebProxy.Credentials = CredentialCache.DefaultCredentials;

            RequestRepo = new RepoFabric(settings, credentials)
                .Create();
        }

        internal IRequestRepo RequestRepo { get; }
        
        /// <summary>
        /// Gets account cookies.
        /// </summary>
        /// <value>Account cookies.</value>
        //public CookieContainer Cookies => _cookies ?? (_cookies = new CookieContainer());

        internal Credentials Credentials { get; }

        public AccountInfoResult Info { get; private set; }

        public bool IsAnonymous => Credentials.IsAnonymous;
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
            if (!IsAnonymous)
                Info = await RequestRepo.AccountInfo();
            return true;
        }

    }

    public delegate string AuthCodeRequiredDelegate(string login, bool isAutoRelogin);
}
