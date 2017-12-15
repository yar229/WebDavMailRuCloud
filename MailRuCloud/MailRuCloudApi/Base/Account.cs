using System.Net;
using System.Threading.Tasks;
using YaR.MailRuCloud.Api.Base.Repos;
using YaR.MailRuCloud.Api.Base.Requests.Types;

namespace YaR.MailRuCloud.Api.Base
{
    //TODO: refact, maybe we don't need this class
    //TODO: refact, Requestrepo - wrong place?
    public class Account
    {
        private static readonly log4net.ILog Logger = log4net.LogManager.GetLogger(typeof(Account));

        private readonly CloudApi _cloudApi;

        /// <summary>
        /// Default cookies.
        /// </summary>
        private CookieContainer _cookies;

        private IRequestRepo _requestRepo;

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

            
        }

        internal IRequestRepo RequestRepo => _requestRepo ??
                                             //(_requestRepo = new MobileRequestRepo(_cloudApi.Account.Proxy, _cloudApi.Account.Credentials)); 
                                             //(_requestRepo = new WebV2RequestRepo(_cloudApi.Account.Proxy, new WebAuth(_cloudApi.Account.Proxy, _cloudApi.Account.Credentials,OnAuthCodeRequired)));
                                            (_requestRepo = new WebM1RequestRepo(_cloudApi.Account.Proxy, _cloudApi.Account.Credentials, OnAuthCodeRequired));
                                            //MixedRepo(_cloudApi));

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

        public AccountInfoResult Info { get; private set; }

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
            //await RequestRepo.Login(OnAuthCodeRequired);

            Info = await RequestRepo.AccountInfo();

            return true;
        }

        public event AuthCodeRequiredDelegate AuthCodeRequiredEvent;
        protected virtual string OnAuthCodeRequired(string login, bool isAutoRelogin)
        {
            return AuthCodeRequiredEvent?.Invoke(login, isAutoRelogin);
        }
    }

    public delegate string AuthCodeRequiredDelegate(string login, bool isAutoRelogin);
}
