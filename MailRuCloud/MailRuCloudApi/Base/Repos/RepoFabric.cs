using System;

namespace YaR.MailRuCloud.Api.Base.Repos
{
    class RepoFabric
    {
        private static readonly log4net.ILog Logger = log4net.LogManager.GetLogger(typeof(RepoFabric));

        private readonly CloudSettings _settings;
        private readonly Credentials _credentials;

        public RepoFabric(CloudSettings settings, Credentials credentials)
        {
            _settings = settings;
            _credentials = credentials;
        }

        public IRequestRepo Create()
        {
            string TwoFaHandler(string login, bool isAutoRelogin)
            {
                Logger.Info($"Waiting 2FA code for {login}");
                var code = _settings.TwoFaHandler?.Get(login, isAutoRelogin);
                Logger.Info($"Got 2FA code for {login}");
                return code;
            }

            IRequestRepo repo;
            switch (_settings.Protocol)
            {
                case Protocol.WebM1Bin:
                    repo = new WebM1RequestRepo(_settings.Proxy, _credentials, TwoFaHandler, _settings.ListDepth);
                    break;
                case Protocol.WebV2:
                    repo = new WebV2RequestRepo(_settings.Proxy, _credentials, TwoFaHandler);
                    break;
                default:
                    throw new Exception("Unknown protocol");
            }

            if (!string.IsNullOrWhiteSpace(_settings.UserAgent))
                repo.HttpSettings.UserAgent = _settings.UserAgent;

            return repo;
        }

        //   ??
        //(_requestRepo = new MobileRequestRepo(_cloudApi.Account.Proxy, _cloudApi.Account.Credentials)); 
        //(_requestRepo = new WebV2RequestRepo(_cloudApi.Account.Proxy, new WebAuth(_cloudApi.Account.Proxy, _cloudApi.Account.Credentials,OnAuthCodeRequired)));
        //------(_requestRepo = new WebM1RequestRepo(Proxy, Credentials, OnAuthCodeRequired));
        //MixedRepo(_cloudApi));

    }
}
