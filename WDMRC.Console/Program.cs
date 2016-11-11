using System;
using System.Net;
using System.Threading;
using CommandLine;
using NWebDav.Server;
using NWebDav.Server.Handlers;
using NWebDav.Server.Http;
using NWebDav.Server.HttpListener;
using NWebDav.Server.Stores;
using WebDavMailRuCloudStore;
using YaR.WebDavMailRu.CloudStore;

namespace YaR.WebDavMailRu
{
    static class Program
    {
        static void Main(string[] args)
        {
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
            var homeFolder = new MailruStore(true);
            var webDavDispatcher = new WebDavDispatcher(homeFolder, requestHandlerFactory);

            // Determine the WebDAV username/password for authorization
            // (only when basic authentication is enabled)
            var webdavUsername = "test";
            var webdavPassword = "test";

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
