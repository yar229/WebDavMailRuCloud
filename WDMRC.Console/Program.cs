using System;
using System.Linq;
using CommandLine;
using WebDAVSharp.Server;

namespace FooConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            var result = CommandLine.Parser.Default.ParseArguments<CommandLineOptions>(args);

            var exitCode = result
              .MapResult(
                options => 
                {

                    var store = new WebDavMailRuCloudStore.WebDavMailRuCloudStore(options.Login, options.Password);
                    var wds = new WebDavServer(store, AuthType.Anonymous);
                    wds.Start($"http://localhost:{options.Port}/");
                    return 0;
                },
                errors => 1);

            if (exitCode > 0) Environment.Exit(exitCode);
        }
    }
}
