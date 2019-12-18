using System.Collections.Concurrent;
using System.Linq;
using System.Net;
using System.Security.Principal;
using YaR.MailRuCloud.Api;
using YaR.MailRuCloud.Api.Base;

namespace YaR.WebDavMailRu.CloudStore
{
    public static class CloudManager
    {
        private static readonly log4net.ILog Logger = log4net.LogManager.GetLogger(typeof(CloudManager));

        private static readonly ConcurrentDictionary<string, MailRuCloud.Api.MailRuCloud> CloudCache = new ConcurrentDictionary<string, MailRuCloud.Api.MailRuCloud>();

        public static CloudSettings Settings { get; set; }

        public static MailRuCloud.Api.MailRuCloud Instance(IIdentity identityi)
        {
            var identity = (HttpListenerBasicIdentity) identityi;
            string key = identity.Name + identity.Password;

            if (CloudCache.TryGetValue(key, out var cloud))
                return cloud;

            lock (Locker)
            {
                if (!CloudCache.TryGetValue(key, out cloud))
                {
                    cloud = CreateCloud(identity);

                    if (!CloudCache.TryAdd(key, cloud))
                        CloudCache.TryGetValue(key, out cloud);
                }
            }

            return cloud;
        }

        private static readonly object Locker = new object();

        private static MailRuCloud.Api.MailRuCloud CreateCloud(HttpListenerBasicIdentity identity)
        {
            Logger.Info($"Cloud instance created for {identity.Name}");

            var credentials = new Credentials(identity.Name, identity.Password);

            var cloud = new MailRuCloud.Api.MailRuCloud(Settings, credentials);
            return cloud;
        }
    }
}
