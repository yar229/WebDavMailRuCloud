using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using YaR.MailRuCloud.Api;

namespace YaR.WebDavMailRu.CloudStore
{
    public static class TwoFaHandlers
    {
        static TwoFaHandlers()
        {
            HandlerTypes = GetHandlers().ToList();
        }

        private static readonly List<Type> HandlerTypes;


        public static ITwoFaHandler Get(string name)
        {
            var type = HandlerTypes.FirstOrDefault(t => t.Name == name);
            if (null == type) return null;

            var inst = (ITwoFaHandler)Activator.CreateInstance(type);
            return inst;
        }

        private static IEnumerable<Type> GetHandlers()
        {
            foreach (var file in Directory.EnumerateFiles(Path.GetDirectoryName(typeof(TwoFaHandlers).Assembly.Location) ?? throw new InvalidOperationException(), "MailRuCloudApi.TwoFA*.dll", SearchOption.TopDirectoryOnly))
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