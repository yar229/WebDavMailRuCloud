using System;
using System.Reflection;

namespace YaR.Clouds
{
    public class CryptFileInfo
    {
        public const string FileName = ".crypt.wdmrc";

        // ReSharper disable once UnusedMember.Global
        public string WDMRCVersion => Assembly.GetExecutingAssembly().GetName().Version?.ToString()
                                      ?? throw new Exception($"{nameof(CryptFileInfo)}.{nameof(WDMRCVersion)} is empty");

        public DateTime Initialized { get; set; }
    }
}