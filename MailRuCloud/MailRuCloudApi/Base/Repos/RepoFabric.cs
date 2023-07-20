using System;
using YaR.Clouds.Base.Repos.MailRuCloud.WebBin;
using YaR.Clouds.Base.Repos.MailRuCloud.WebV2;

namespace YaR.Clouds.Base.Repos
{
    public class RepoFabric
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

            IRequestRepo repo = _settings.Protocol switch
            {
                Protocol.YadWebV2 => new YandexDisk.YadWebV2.YadWebRequestRepo(_settings, _settings.Proxy, _credentials),
                Protocol.YadWeb => new YandexDisk.YadWeb.YadWebRequestRepo(_settings, _settings.Proxy, _credentials),
                Protocol.WebM1Bin => new WebBinRequestRepo(_settings, _credentials, TwoFaHandler),
                Protocol.WebV2 => new WebV2RequestRepo(_settings, _credentials, TwoFaHandler),
                _ => throw new Exception("Unknown protocol")
            };

            if (!string.IsNullOrWhiteSpace(_settings.UserAgent))
                repo.HttpSettings.UserAgent = _settings.UserAgent;

            return repo;
        }
    }
}
