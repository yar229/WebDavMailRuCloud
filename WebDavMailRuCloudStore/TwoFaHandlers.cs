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
            var files = Directory.EnumerateFiles(
                Path.GetDirectoryName(typeof(TwoFaHandlers).Assembly.Location) ?? throw new InvalidOperationException(),
                "MailRuCloud.TwoFA*.dll",
                SearchOption.TopDirectoryOnly);
            foreach (var file in files)
            {
                //If an application has been copied from the web, it is flagged by Windows as being a web application, even if it resides on the local computer. 
                //You can change that designation by changing the file properties, or you can use the element to grant the assembly full trust. 
                //As an alternative, you can use the UnsafeLoadFrom method to load a local assembly that the operating system has flagged as having been loaded from the web.
                Assembly assembly = Assembly.UnsafeLoadFrom(file);
                foreach (var type in assembly.ExportedTypes)
                {
                    if (type.GetInterfaces().Contains(typeof(ITwoFaHandler)))
                        yield return type;
                }
            }
        }
    }
}