using Microsoft.AspNetCore.Hosting;

using NWebDav.Server.Logging;

using NWebDav.Sample.Kestrel.LogAdapters;

namespace NWebDav.Sample.Kestrel
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            // Use debug output for logging
            var adapter = new DebugOutputAdapter();
            adapter.LogLevels.Add(LogLevel.Debug);
            adapter.LogLevels.Add(LogLevel.Info);
            LoggerFactory.Factory = adapter;

            var host = new WebHostBuilder()
                .UseKestrel(options => {
                    options.ThreadCount = 4;
                    //options.UseConnectionLogging();
                })
                .UseStartup<Startup>()
                .Build();

            host.Run();
        }
    }
}