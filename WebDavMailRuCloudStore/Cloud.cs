using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Runtime.CompilerServices;
using MailRuCloudApi;
using MailRuCloudApi.Api;
using NWebDav.Server.Handlers;
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

        public static MailRuCloud Instance(IHttpContext context)
        {
            HttpListenerBasicIdentity identity = (HttpListenerBasicIdentity)context.Session.Principal.Identity;
            string key = identity.Name + identity.Password;

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

            var twoFaHandler = TwoFaHandlers.Get("AuthCodeConsole");
            cloud = new SplittedCloud(identity.Name, identity.Password, twoFaHandler);
            if (!CloudCache.TryAdd(key, cloud))
                CloudCache.TryGetValue(key, out cloud);


            return cloud;
        }
    }


    public static class TwoFaHandlers
    {
        static TwoFaHandlers()
        {
            _handlerTypes = GetHandlers().ToList();
        }

        private static List<Type> _handlerTypes;


        public static ITwoFaHandler Get(string name)
        {
            var type = _handlerTypes.FirstOrDefault(t => t.Name == name);
            if (null == type) return null;

            var inst = (ITwoFaHandler)Activator.CreateInstance(type);
            return inst;
        }

        private static IEnumerable<Type> GetHandlers()
        {
            foreach (var file in Directory.EnumerateFiles(Path.GetDirectoryName(typeof(TwoFaHandlers).Assembly.Location), "MailRuCloudApi.TwoFA*.dll", SearchOption.TopDirectoryOnly))
            {
                Assembly assembly = Assembly.LoadFile(file);
                foreach (var type in assembly.ExportedTypes)
                {
                    if (type.GetInterfaces().Contains(typeof(ITwoFaHandler)))
                        yield return type;
                }
            }
        }
    }
}
