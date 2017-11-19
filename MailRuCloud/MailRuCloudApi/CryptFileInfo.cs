using System;
using System.Reflection;

namespace YaR.MailRuCloud.Api
{
    public class CryptFileInfo
    {
        public const string FileName = ".crypt.wdmrc";
        public string WDMRCVersion => Assembly.GetExecutingAssembly().GetName().Version.ToString();
        public DateTime Initialized { get; set; }

    }
}