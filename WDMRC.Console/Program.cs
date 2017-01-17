using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using CommandLine;
using NWebDav.Server;
using NWebDav.Server.Http;
using NWebDav.Server.HttpListener;
using NWebDav.Server.Logging;
using YaR.WebDavMailRu.CloudStore;
using YaR.WebDavMailRu.CloudStore.Mailru.StoreBase;

namespace YaR.WebDavMailRu
{
    static class Program
    {
        private static readonly log4net.ILog Logger;

        static Program()
        {
            log4net.Config.XmlConfigurator.Configure();
            Logger = log4net.LogManager.GetLogger(typeof(Program));
        }


        static void Main(string[] args)
        {
            LoggerFactory.Factory = new Log4NetAdapter();

            var result = Parser.Default.ParseArguments<CommandLineOptions>(args);

            var exitCode = result
              .MapResult(
                options => 
                {
                    Cloud.Init(options.Login, options.Password);

                    var webdavProtocol = "http";
                    var webdavIp = "127.0.0.1";
                    var webdavPort = options.Port;

                    using (var httpListener = new HttpListener())
                    {
                        httpListener.Prefixes.Add($"{webdavProtocol}://{webdavIp}:{webdavPort}/");

                        // Use basic authentication if requested
                        var webdavUseAuthentication = false;
                        if (webdavUseAuthentication)
                        {
                            httpListener.AuthenticationSchemes = AuthenticationSchemes.Basic;
                            httpListener.Realm = "WebDAV server";
                        }
                        else
                        {
                            httpListener.AuthenticationSchemes = AuthenticationSchemes.Anonymous;
                        }

                        // Start the HTTP listener
                        httpListener.Start();

                        // Start dispatching requests
                        var cancellationTokenSource = new CancellationTokenSource();
                        DispatchHttpRequestsAsync(httpListener, cancellationTokenSource.Token, options.MaxThreadCount);

                        // Wait until somebody presses return
                        Console.WriteLine("WebDAV server running. Press 'x' to quit.");
                        while (Console.ReadKey().KeyChar != 'x') {}

                        cancellationTokenSource.Cancel();

                        return 0;
                    }

                },
                errors => 1);

            if (exitCode > 0) Environment.Exit(exitCode);
        }


        private static async void DispatchHttpRequestsAsync(HttpListener httpListener, CancellationToken cancellationToken, int maxThreadCount = Int32.MaxValue)
        {
            // Create a request handler factory that uses basic authentication
            var requestHandlerFactory = new CloudStore.Mailru.RequestHandlerFactory();

            // Create WebDAV dispatcher
            var homeFolder = new MailruStore();
            var webDavDispatcher = new WebDavDispatcher(homeFolder, requestHandlerFactory);

            // Determine the WebDAV username/password for authorization
            // (only when basic authentication is enabled)
            var webdavUsername = "test";
            var webdavPassword = "test";


            try
            {

                using (var sem = new SemaphoreSlim(maxThreadCount))
                {
                    var semclo = sem;
                    HttpListenerContext httpListenerContext;
                    while (
                        !cancellationToken.IsCancellationRequested &&
                        (httpListenerContext = await httpListener.GetContextAsync().ConfigureAwait(false)) != null
                    )
                    {
                        IHttpContext httpContext;
                        if (httpListenerContext.Request.IsAuthenticated)
                            httpContext = new HttpBasicContext(httpListenerContext,
                                i => i.Name == webdavUsername && i.Password == webdavPassword);
                        else httpContext = new HttpContext(httpListenerContext);

                        //var r = httpContext.Request;
                        //var range = r.GetRange();
                        //Logger.Info($"HTTP {r.Url} {r.HttpMethod} ");
                        //await webDavDispatcher.DispatchRequestAsync(httpContext);

                        await semclo.WaitAsync(cancellationToken);
                        Task tsk = Task
                            .Run(async () =>
                            {
                                try
                                {
                                    //var r = httpContext.Request;
                                    //var range = r.GetRange();
                                    //Logger.Info($"HTTP {r.Url} {r.HttpMethod} ");
                                    //if (null != range) Logger.Info($"Range {range.Start} / {range.End} {range.If}");
                                    //Logger.Info($"-------awail {semclo.CurrentCount}");

                                    await webDavDispatcher.DispatchRequestAsync(httpContext);

                                    //Logger.Info($"-------awail {semclo.CurrentCount}");
                                }
                                catch (Exception ex)
                                {
                                    Logger.Error("Exception", ex);
                                }

                            }, cancellationToken)
                            .ContinueWith(t => semclo.Release(), cancellationToken);
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Error("Global exception", e);
            }


        }
    }
}
