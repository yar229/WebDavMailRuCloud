using System;
using System.Reflection;
using CommandLine;
using WinServiceInstaller;
using YaR.WebDavMailRu;

namespace YaR.CloudMailRu.Console
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
                            Name = options.ServiceName ?? options.ServiceUninstall ?? "wdmrc",
                            DisplayName = "WebDavMailRuCloud",
                            Description = "WebDAV proxy for Cloud mail.ru",
                            CommandLine = "--service",

                            FireStart = () => Payload.Run(options),
                            FireStop = () => Payload.CancellationTokenSource.Cancel(),

                        };


                        if (options.ServiceName != null)
                        {
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


                        Payload.Run(options);
                        return 0;
                    },
                    errors => 1);

            if (exitCode > 0) Environment.Exit(exitCode);
        }

    }
}
