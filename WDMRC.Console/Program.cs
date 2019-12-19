using System;
using System.Reflection;
using CommandLine;
using WinServiceInstaller;

namespace YaR.Clouds.Console
{
    public class Program
    {
        private static ServiceConfigurator _c;

        static void Main(string[] args)
        {
            var result = Parser.Default.ParseArguments<CommandLineOptions>(args);
            
            var exitCode = result
                .MapResult(
                    options =>
                    {
                        _c = new ServiceConfigurator
                        {
                            Assembly = Assembly.GetExecutingAssembly(),
                            Name = options.ServiceInstall ?? options.ServiceUninstall ?? "wdmrc",
                            DisplayName = "WebDavMailRuCloud",
                            Description = "WebDAV proxy for Cloud mail.ru",

                            FireStart = () => Payload.Run(options),
                            FireStop = Payload.Stop

                        };

                        if (options.ServiceInstall != null)
                        {
                            options.ServiceRun = true;
                            options.ServiceInstall = null;
                            _c.CommandLine = Parser.Default.FormatCommandLine(options);

                            _c.Install();
                            return 0;
                        }
                        else if (options.ServiceUninstall != null)
                        {
                            _c.Uninstall();
                            return 0;
                        }
                        else if (options.ServiceRun)
                        {
                            _c.Run();
                            return 0;
                        }

                        System.Console.CancelKeyPress += (sender, eventArgs) => Payload.Stop();
                        Payload.Run(options);
                        return 0;
                    },
                    errors => 1);

            if (exitCode > 0) Environment.Exit(exitCode);
        }

    }
}
