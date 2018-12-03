using System;
using System.IO;
using System.Xml;

namespace YaR.CloudMailRu.Console
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

        public static XmlElement Log4Net => (XmlElement)Document.SelectSingleNode("/config/log4net");

        public static string TwoFactorAuthHandlerName
        {
            get
            {
                var res = Document.SelectSingleNode("/config/TwoFactorAuthHandlerName").InnerText;
                return res;
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
    }
}