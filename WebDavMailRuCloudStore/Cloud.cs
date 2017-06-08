using System;
using System.Collections.Concurrent;
using System.Net;
using MailRuCloudApi;
using MailRuCloudApi.Api;
using MailRuCloudApi.CookieManager;
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
        public static ICookieManager CookieManager { get; set; }

        private static readonly object Locker = new object();

        public static MailRuCloud Instance(IHttpContext context)
        {
            HttpListenerBasicIdentity identity = (HttpListenerBasicIdentity)context.Session.Principal.Identity;
            string key = identity.Name + identity.Password;

            //TODO: rewrite this cruel lock
            lock (Locker)
            { 
                MailRuCloud cloud;
                if (CloudCache.TryGetValue(key, out cloud))
                {
                    if (cloud.CloudApi.Account.Expires <= DateTime.Now)
                        CloudCache.TryRemove(key, out cloud);
                    else
                        return cloud;
                }

                if (!identity.Name.Contains("@mail."))
                    Logger.Warn("Missing domain part (@mail.*) in login, file and folder deleting will be denied");


                //2FA
                ITwoFaHandler twoFaHandler = null;

                if (!string.IsNullOrEmpty(TwoFactorHandlerName))
                {
                    twoFaHandler = TwoFaHandlers.Get(TwoFactorHandlerName);
                    if (null == twoFaHandler)
                        Logger.Error($"Cannot load two-factor auth handler {TwoFactorHandlerName}");
                }

                cloud = new SplittedCloud(identity.Name, identity.Password, twoFaHandler, CookieManager?.Load(identity.Name));
                if (!CloudCache.TryAdd(key, cloud))
                    CloudCache.TryGetValue(key, out cloud);

                return cloud;
            }
        }

        public static void SaveCookies()
        {
            if (null == CookieManager) return;

            foreach (var cloud in CloudCache.Values)
                CookieManager.Save(cloud.CloudApi.Account.LoginName, cloud.CloudApi.Account.Cookies);
        }

    }
}
