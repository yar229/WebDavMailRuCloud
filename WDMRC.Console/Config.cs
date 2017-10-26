using System.IO;
using System.Xml;

namespace YaR.CloudMailRu.Console
{
    internal static class Config
    {
        static Config()
        {
            Document = new XmlDocument();
            Document.Load(File.OpenRead("wdmrc.config"));
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
    }
}