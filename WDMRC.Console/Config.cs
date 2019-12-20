using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using YaR.Clouds.Common;
using YaR.Clouds.Extensions;
using YaR.Clouds.WebDavStore;

namespace YaR.Clouds.Console
{
    internal static class Config
    {
        static Config()
        {
            Document = new XmlDocument();
            var configpath = Path.Combine(Path.GetDirectoryName(typeof(Config).Assembly.Location), "wdmrc.config");
            Document.Load(File.OpenRead(configpath));
        }

        private static readonly XmlDocument Document;

        //public static XmlElement Log4Net => (XmlElement)Document.SelectSingleNode("/config/log4net");
        public static XmlElement Log4Net
        {
            get
            {
                var e = (XmlElement) Document.SelectSingleNode("/config/log4net");
                var nz = e?.SelectNodes("appender/file");
                if (nz != null)
                    foreach (XmlNode eChildNode in nz)
                    {
                        var attr = eChildNode.Attributes?["value"];
                        if (attr != null)
                            attr.Value = attr.Value
                                .Replace('/', Path.DirectorySeparatorChar)
                                .Replace('\\', Path.DirectorySeparatorChar);
                    }
                return e;
            }
        }

        public static TwoFactorAuthHandlerInfo TwoFactorAuthHandler
        {
            get
            {
                var info = new TwoFactorAuthHandlerInfo();

                var node = Document.SelectSingleNode("/config/TwoFactorAuthHandler");
                info.Name = node.Attributes["Name"].InnerText;
                var parames = new List<KeyValuePair<string, string>>();
                foreach (XmlNode childNode in node.ChildNodes)
                {
                    string pname = childNode.Attributes["Name"].InnerText;
                    string pvalue = childNode.Attributes["Value"].InnerText;
                    parames.Add(new KeyValuePair<string, string>(pname, pvalue));
                }

                info.Parames = parames;


                return info;
            }
        }


        public static string SpecialCommandPrefix => ">>";

        public static string AdditionalSpecialCommandPrefix
        {
            get
            {
                try
                {
                    var res = Document.SelectSingleNode("/config/AdditionalSpecialCommandPrefix").InnerText;
                    return res;
                }
                catch (Exception)
                {
                    return null;
                }

            }
        }

        public static SharedVideoResolution DefaultSharedVideoResolution
        {
            get
            {
                try
                {
                    string evalue = Document.SelectSingleNode("/config/DefaultSharedVideoResolution").InnerText;
                    var res = EnumExtensions.ParseEnumMemberValue<SharedVideoResolution>(evalue);
                    return res;
                }
                catch (Exception)
                {
                    return SharedVideoResolution.All;
                }
            }
        }
    }
}