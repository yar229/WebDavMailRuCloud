using System;
using System.Reflection;

namespace YaR.MailRuCloud.Api
{
    public class CryptFileInfo
    {
        public const string FileName = ".crypt.wdmrc";

        // ReSharper disable once UnusedMember.Global
        public string WDMRCVersion => Assembly.GetExecutingAssembly().GetName().Version.ToString();

        public DateTime Initialized { get; set; }
    }
}