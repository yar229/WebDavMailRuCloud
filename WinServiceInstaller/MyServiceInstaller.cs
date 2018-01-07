using System.ComponentModel;
using System.Configuration.Install;
using System.ServiceProcess;

namespace WinServiceInstaller
{
    [RunInstaller(true)]
    class MyServiceInstaller : Installer
    {
        public MyServiceInstaller()
        {
            // Instantiate installers for process and services.
            var processInstaller = new ServiceProcessInstaller
            {
                Account = ServiceAccount.LocalSystem
            };

            var serviceInstaller = new ServiceInstaller
            {
                StartType = ServiceStartMode.Automatic,
                ServiceName = ServiceName
            };

            Installers.Add(serviceInstaller);
            Installers.Add(processInstaller);
        }

        public static string ServiceName { get; set; }
    }
}