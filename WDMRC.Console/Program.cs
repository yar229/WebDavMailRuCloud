using System;
using System.Linq;
using System.Net;
using System.Threading;
using System.Xml;
using CommandLine;
using NWebDav.Server;
using NWebDav.Server.Handlers;
using NWebDav.Server.Http;
using NWebDav.Server.HttpListener;
using NWebDav.Server.Logging;
using NWebDav.Server.Stores;
using WebDavMailRuCloudStore;

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

                    //var store = new WebDavMailRuCloudStore.WebDavMailRuCloudStore(options.Login, options.Password);
                    //var wds = new WebDavServer(store, AuthType.Anonymous);
                    //wds.Start($"http://localhost:{options.Port}/");
                    //return 0;

                    // Use the Log4NET adapter for logging
                    //LoggerFactory.Factory = new Log4NetAdapter();

                    Cloud.Init(options.Login, options.Password);


                    // Obtain the HTTP binding settings
                    var webdavProtocol = "http"; //ConfigurationManager.AppSettings["webdav.protocol"] ?? "http";
                    var webdavIp = "127.0.0.1"; //ConfigurationManager.AppSettings["webdav.ip"] ?? "127.0.0.1";
                    var webdavPort = options.Port; //ConfigurationManager.AppSettings["webdav.port"] ?? "11111";

                    using (var httpListener = new HttpListener())
                    {
                        // Add the prefix
                        httpListener.Prefixes.Add($"{webdavProtocol}://{webdavIp}:{webdavPort}/");

                        // Use basic authentication if requested
                        var webdavUseAuthentication = false; //XmlConvert.ToBoolean(ConfigurationManager.AppSettings["webdav-authentication"] ?? "false");
                        if (webdavUseAuthentication)
                        {
                            //// Check if HTTPS is enabled
                            //if (webdavProtocol != "https" && s_log.IsWarnEnabled)
                            //    s_log.Warn("Most WebDAV clients cannot use authentication on a non-HTTPS connection");

                            // Set the authentication scheme and realm
                            httpListener.AuthenticationSchemes = AuthenticationSchemes.Basic;
                            httpListener.Realm = "WebDAV server";
                        }
                        else
                        {
                            // Allow anonymous access
                            httpListener.AuthenticationSchemes = AuthenticationSchemes.Anonymous;
                        }

                        // Start the HTTP listener
                        httpListener.Start();

                        // Start dispatching requests
                        var cancellationTokenSource = new CancellationTokenSource();
                        DispatchHttpRequestsAsync(httpListener, cancellationTokenSource.Token);

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


        private static async void DispatchHttpRequestsAsync(System.Net.HttpListener httpListener, CancellationToken cancellationToken)
        {
            // Create a request handler factory that uses basic authentication
            var requestHandlerFactory = new RequestHandlerFactory();

            // Create WebDAV dispatcher
            var homeFolder = new MailruStore(true);//Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            var webDavDispatcher = new WebDavDispatcher(homeFolder, requestHandlerFactory);

            // Determine the WebDAV username/password for authorization
            // (only when basic authentication is enabled)
            var webdavUsername = "test"; //ConfigurationManager.AppSettings["webdav.username"] ?? "test";
            var webdavPassword = "test"; //ConfigurationManager.AppSettings["webdav.password"] ?? "test";

            HttpListenerContext httpListenerContext;
            while (!cancellationToken.IsCancellationRequested && (httpListenerContext = await httpListener.GetContextAsync().ConfigureAwait(false)) != null)
            {
                // Determine the proper HTTP context
                IHttpContext httpContext;
                if (httpListenerContext.Request.IsAuthenticated)
                    httpContext = new HttpBasicContext(httpListenerContext, checkIdentity: i => i.Name == webdavUsername && i.Password == webdavPassword);
                    //httpContext = new HttpBasicContext(httpListenerContext, checkIdentity: i => i == null || (i.Name == webdavUsername && i.Password == webdavPassword));

                else
                    httpContext = new HttpContext(httpListenerContext);

                // Dispatch the request
                await webDavDispatcher.DispatchRequestAsync(httpContext).ConfigureAwait(false);
            }
        }
    }
}
