using System;
using YaR.Clouds.Base.Repos.MailRuCloud.WebBin;
using YaR.Clouds.Base.Repos.MailRuCloud.WebV2;
using YaR.Clouds.Base.Repos.YandexDisk.YadWeb;

namespace YaR.Clouds.Base.Repos
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
                case Protocol.YadWeb:
                    repo = new YadWebRequestRepo(_settings.Proxy, _credentials);
                    break;
                case Protocol.WebM1Bin:
                    repo = new WebBinRequestRepo(_settings.Proxy, _credentials, TwoFaHandler);
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
    }
}
