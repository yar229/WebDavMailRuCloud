using System.Collections.Concurrent;
using System.Net;
using System.Security.Principal;
using YaR.Clouds.Base;

namespace YaR.Clouds.WebDavStore
{
    public static class CloudManager
    {
        private static readonly log4net.ILog Logger = log4net.LogManager.GetLogger(typeof(CloudManager));

        private static readonly ConcurrentDictionary<string, Cloud> CloudCache = new();

        public static CloudSettings Settings { get; set; }

        public static Cloud Instance(IIdentity identityi)
        {
            var identity = (HttpListenerBasicIdentity) identityi;
            string key = identity.Name + identity.Password;

            if (CloudCache.TryGetValue(key, out var cloud))
                return cloud;

            lock (Locker)
            {
                if (CloudCache.TryGetValue(key, out cloud)) 
                    return cloud;

                cloud = CreateCloud(identity);

                if (!CloudCache.TryAdd(key, cloud))
                    CloudCache.TryGetValue(key, out cloud);
            }

            return cloud;
        }

        private static readonly object Locker = new();

        private static Cloud CreateCloud(HttpListenerBasicIdentity identity)
        {
            Logger.Info($"Cloud instance created for {identity.Name}");

            var credentials = new Credentials(identity.Name, identity.Password);

            var cloud = new Cloud(Settings, credentials);
            return cloud;
        }
    }
}
