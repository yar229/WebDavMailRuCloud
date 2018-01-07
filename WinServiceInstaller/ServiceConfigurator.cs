using System;
using System.Configuration.Install;
using System.Reflection;
using System.ServiceProcess;

namespace WinServiceInstaller
{
    public class ServiceConfigurator
    {
        private string _name;
        public Assembly Assembly { get; set; }

        public string Name
        {
            get => _name;
            set
            {
                MyServiceInstaller.ServiceName = value;
                _name = value;
            }
        }

        public string DisplayName { get; set; }
        public string Description { get; set; }
        public string CommandLine { get; set; }

        public Action FireStart { get; set; }
        public Action FireStop { get; set; }


        public void Install()
        {
            ManagedInstallerClass.InstallHelper(new[] {Assembly.Location});

            if (!string.IsNullOrWhiteSpace(CommandLine))
                AddCommandLineParametersToStartupOptions(CommandLine);
        }

        public void Run()
        {
            using (var service = new StubService
            {
                ServiceName = Name,
                FireStart = FireStart,
                FireStop = FireStop
            })
            {
                ServiceBase.Run(service);
            }
        }

        public void Uninstall()
        {
            ManagedInstallerClass.InstallHelper(new[] { "/u", Assembly.Location });
        }


        private void AddCommandLineParametersToStartupOptions(string args)
        {
            var serviceKey = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(
                $"SYSTEM\\CurrentControlSet\\Services\\{Name}",
                true);

            if (serviceKey == null)
            {
                throw new Exception($"Could not locate Registry Key for service '{Name}'");
            }

            var cmd = serviceKey.GetValue("ImagePath");
            serviceKey.SetValue("ImagePath", $"{cmd} {args}", Microsoft.Win32.RegistryValueKind.String);
        }
    }
}