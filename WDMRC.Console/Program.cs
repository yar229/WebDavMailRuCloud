using System;
using System.ComponentModel;
using System.Threading.Tasks;

namespace YaR.CloudMailRu.Console
{
#if NET45
    [DesignerCategory("")]
    [AC0KG.WindowsService.ServiceName("wdmrc")]
    class Service : AC0KG.WindowsService.ServiceShell { }

    [RunInstaller(true)]
    [AC0KG.WindowsService.ServiceName("wdmrc",
        DisplayName = "WebDavMailRuCloud",
        Description = "WebDAV proxy for Cloud mail.ru Service")]
    public class Installer : AC0KG.WindowsService.InstallerShell { }
#endif

    public class Program 
    {
        static void Main(string[] args)
        {
#if NET45
            if (AC0KG.WindowsService.ServiceShell.ProcessInstallOptions(args))
                return;

            Service.StartService<Service>(
                () => { Task.Factory.StartNew(() => Payload.Run(args), Payload.CancellationTokenSource.Token); },
                () => { Payload.CancellationTokenSource.Cancel(); },
                Environment.UserInteractive);
#else
            Payload.Run(args);
#endif
        }
    }
}
