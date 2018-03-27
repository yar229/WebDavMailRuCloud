using System;
using System.Net;

namespace YaR.MailRuCloud.Api.Base.Repos
{
    class RepoFabric
    {
        private readonly CloudSettings _settings;
        private readonly Credentials _credentials;
        private readonly IWebProxy _proxy;

        public RepoFabric(CloudSettings settings, Credentials credentials, IWebProxy proxy)
        {
            _settings = settings;
            _credentials = credentials;
            _proxy = proxy;
        }

        public IRequestRepo Create()
        {
            string TwoFaHandler(string login, bool isAutoRelogin) => _settings.TwoFaHandler?.Get(login, isAutoRelogin);

            IRequestRepo repo;
            switch (_settings.Protocol)
            {
                case Protocol.WebM1Bin:
                    repo = new WebM1RequestRepo(_proxy, _credentials, TwoFaHandler, _settings.ListDepth);
                    break;
                case Protocol.WebV2:
                    repo = new WebV2RequestRepo(_proxy, _credentials, TwoFaHandler);
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
