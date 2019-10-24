using System;
using System.Reflection;

#if NET461
using System.Configuration.Install;
using System.ServiceProcess;

namespace WinServiceInstaller
{
    public class ServiceConfigurator
    {
        private string _name;
        private string _displayName;
        private string _description;
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

        public string DisplayName
        {
            get => _displayName;
            set
            {
                MyServiceInstaller.DisplayName = value;
                _displayName = value;
            }
        }

        public string Description
        {
            get => _description;
            set
            {
                MyServiceInstaller.Description = value;
                _description = value;
            }
        }

        public string CommandLine { get; set; }

        public Action FireStart { get; set; }
        public Action FireStop { get; set; }

        public void Install()
        {
            ManagedInstallerClass.InstallHelper(new[] { Assembly.GetExecutingAssembly().Location });

            string cmd = Assembly.Location;
            if (!string.IsNullOrWhiteSpace(CommandLine))
                cmd += " " + CommandLine;
            SetCommandLine(cmd);
        }

        public void Run()
        {
                var service = new StubService
                {
                    ServiceName = "wdmrc",
                    FireStart = FireStart,
                    FireStop = FireStop
                };

                ServiceBase.Run(new ServiceBase[] {service});
        }

        public void Uninstall()
        {
            string cmd = Assembly.GetExecutingAssembly().Location;
            SetCommandLine(cmd);
            ManagedInstallerClass.InstallHelper(new[] { "/u", cmd });
        }


        private void SetCommandLine(string cmd)
        {
            var serviceKey = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(
                $"SYSTEM\\CurrentControlSet\\Services\\{Name}",
                true);

            if (serviceKey == null)
            {
                throw new Exception($"Could not locate Registry Key for service '{Name}'");
            }

            //var cmd = serviceKey.GetValue("ImagePath");
            serviceKey.SetValue("ImagePath", cmd, Microsoft.Win32.RegistryValueKind.String);
        }
    }
}
#else

namespace WinServiceInstaller
{
    public class ServiceConfigurator
    {
        public Assembly Assembly { get; set; }
        public string Name { get; set; }
        public string DisplayName { get; set; }
        public string Description { get; set; }
        public string CommandLine { get; set; }

        public Action FireStart { get; set; }
        public Action FireStop { get; set; }

        public void Install()
        {
        }

        public void Run()
        {
        }

        public void Uninstall()
        {
        }
    }
}


#endif