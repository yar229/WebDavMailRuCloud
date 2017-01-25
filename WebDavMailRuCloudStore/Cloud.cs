using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using MailRuCloudApi;
using MailRuCloudApi.Api;
using NWebDav.Server.Http;

namespace YaR.WebDavMailRu.CloudStore
{
    public static class Cloud
    {
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

            cloud = new SplittedCloud(identity.Name, identity.Password);
            if (!CloudCache.TryAdd(key, cloud))
                CloudCache.TryGetValue(key, out cloud);


            return cloud;
        }


    }
}
