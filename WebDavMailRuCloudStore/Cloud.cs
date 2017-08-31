using System;
using System.Collections.Concurrent;
using System.IO;
using System.Net;
using System.Runtime.Serialization.Formatters.Soap;
using MailRuCloudApi;
using MailRuCloudApi.Api;
using NWebDav.Server.Http;
using File = MailRuCloudApi.File;

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

                CookieContainer cc = LoadCoockies(identity.Name);
                cloud = new SplittedCloud(identity.Name, identity.Password, twoFaHandler, cc);
                if (!CloudCache.TryAdd(key, cloud))
                    CloudCache.TryGetValue(key, out cloud);
            

                return cloud;
            }
        }

        private static string _appdatapath {
            get
            {
                string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "WDMRC\\Cookies");
                if (!Directory.Exists(path)) Directory.CreateDirectory(path);
                return path;
            }
        }
            

        public static void SaveCoockies()
        {
            foreach (var cloud in CloudCache.Values)
            {

                var formatter = new SoapFormatter();
                string file = Path.Combine(_appdatapath, $"{cloud.CloudApi.Account.LoginName}.coockie.dat");

                using (Stream s = System.IO.File.Create(file))
                    formatter.Serialize(s, cloud.CloudApi.Account.Cookies);
            }
        }

        private static CookieContainer LoadCoockies(string login)
        {
            var formatter = new SoapFormatter();
            CookieContainer retrievedCookies = null;
            string file = Path.Combine(_appdatapath, $"{login}.coockie.dat");
            if (System.IO.File.Exists(file))
                using (Stream s = System.IO.File.OpenRead(file))
                    retrievedCookies = (CookieContainer)formatter.Deserialize(s);

            return retrievedCookies;
        }
    }
}
