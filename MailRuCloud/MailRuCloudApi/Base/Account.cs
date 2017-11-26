using System.Net;
using System.Threading.Tasks;
using YaR.MailRuCloud.Api.Base.Requests.Repo;
using YaR.MailRuCloud.Api.Base.Requests.Types;

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

            RequestRepo = new MixedRepo(cloudApi);
        }


        internal IRequestRepo RequestRepo { get; }

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
            await RequestRepo.Login(OnAuthCodeRequired);

            Info = await RequestRepo.AccountInfo();

            return true;
        }



        public delegate string AuthCodeRequiredDelegate(string login, bool isAutoRelogin);

        public event AuthCodeRequiredDelegate AuthCodeRequiredEvent;
        protected virtual string OnAuthCodeRequired(string login, bool isAutoRelogin)
        {
            return AuthCodeRequiredEvent?.Invoke(login, isAutoRelogin);
        }
    }
}
