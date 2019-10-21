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
        //private static readonly log4net.ILog Logger = log4net.LogManager.GetLogger(typeof(Account));

        /// <summary>
        /// Default cookies.
        /// </summary>
        private CookieContainer _cookies;

        /// <summary>
        /// Initializes a new instance of the <see cref="Account" /> class.
        /// </summary>
        public Account(CloudSettings settings, Credentials credentials)
        {
            Credentials = credentials;

            WebRequest.DefaultWebProxy.Credentials = CredentialCache.DefaultCredentials;
            Proxy = WebRequest.DefaultWebProxy;

            RequestRepo = new RepoFabric(settings, credentials, Proxy)
                .Create();
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
