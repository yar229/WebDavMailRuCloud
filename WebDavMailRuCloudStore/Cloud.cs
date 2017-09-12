using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Net;
using MailRuCloudApi;
using MailRuCloudApi.Api;
using NWebDav.Server.Http;

namespace YaR.WebDavMailRu.CloudStore
{
    public static class Cloud
    {
        private static readonly log4net.ILog Logger = log4net.LogManager.GetLogger(typeof(SplittedCloud));

        public static void Init(string userAgent = "")
        {
            if (!string.IsNullOrEmpty(userAgent))
                ConstSettings.UserAgent = userAgent;
        }

        private static readonly ConcurrentDictionary<string, MailRuCloud> CloudCache = new ConcurrentDictionary<string, MailRuCloud>();

        public static string TwoFactorHandlerName { get; set; }

        public static MailRuCloud Instance(IHttpContext context)
        {
            HttpListenerBasicIdentity identity = (HttpListenerBasicIdentity)context.Session.Principal.Identity;
            string key = identity.Name + identity.Password;

            if (CloudCache.TryGetValue(key, out var cloud))
            {
                if (cloud.CloudApi.Account.Expires <= DateTime.Now)
                    CloudCache.TryRemove(key, out cloud);
                else
                    return cloud;
            }

            if (!ConstSettings.AvailDomains.Any(d => identity.Name.Contains($"@{d}.")))
            {
                string domains = ConstSettings.AvailDomains.Aggregate((c, n) => c + ", @" + n);
                Logger.Warn($"Missing domain part ({domains}) in login, file and folder deleting will be denied");
            }


            //2FA
            ITwoFaHandler twoFaHandler = null;

            if (!string.IsNullOrEmpty(TwoFactorHandlerName))
            {
                twoFaHandler = TwoFaHandlers.Get(TwoFactorHandlerName);
                if (null == twoFaHandler)
                    Logger.Error($"Cannot load two-factor auth handler {TwoFactorHandlerName}");
            }

            cloud = new SplittedCloud(identity.Name, identity.Password, twoFaHandler);
            if (!CloudCache.TryAdd(key, cloud))
                CloudCache.TryGetValue(key, out cloud);


            return cloud;
        }
    }
}
