using System;
using System.Globalization;
using System.Net;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using CommandLine;
using NWebDav.Server;
using NWebDav.Server.Http;
using NWebDav.Server.HttpListener;
using NWebDav.Server.Logging;
using YaR.WebDavMailRu;
using YaR.WebDavMailRu.CloudStore;
using YaR.WebDavMailRu.CloudStore.Mailru.StoreBase;

namespace YaR.CloudMailRu.Console
{
    class Payload
    {
        private static readonly log4net.ILog Logger = log4net.LogManager.GetLogger(typeof(Program));

        public static readonly CancellationTokenSource CancellationTokenSource = new CancellationTokenSource();

        public static void Run(string[] args)
        {
            // trying to fix "infinite recursion during resource lookup with system.private.corelib"
            // .Net Core 2.0.0
            Thread.CurrentThread.CurrentUICulture = new CultureInfo("en-us");
            Thread.CurrentThread.CurrentCulture = new CultureInfo("en-us");

            var repo = log4net.LogManager.CreateRepository(Assembly.GetEntryAssembly(), typeof(log4net.Repository.Hierarchy.Hierarchy));
            log4net.Config.XmlConfigurator.Configure(repo, Config.Log4Net);

            LoggerFactory.Factory = new Log4NetAdapter();

            var result = Parser.Default.ParseArguments<CommandLineOptions>(args);

            var exitCode = result
              .MapResult(
                options =>
                {
                    ShowInfo(options);
                    CloudManager.Init();
                    CloudManager.TwoFactorHandlerName = Config.TwoFactorAuthHandlerName;

                    var webdavProtocol = "http";
                    var webdavIp = "127.0.0.1";
                    var webdavPort = options.Port;
                    var webdavHost = string.IsNullOrWhiteSpace(options.Host)
                        ? $"{webdavProtocol}://{webdavIp}"
                        : options.Host.TrimEnd('/');
                    if (webdavHost.EndsWith("//0.0.0.0")) webdavHost = webdavHost.Replace("//0.0.0.0", "//*");


                    var httpListener = new HttpListener();
                    try
                    {
                        httpListener.Prefixes.Add($"{webdavHost}:{webdavPort}/");
                        httpListener.AuthenticationSchemes = AuthenticationSchemes.Basic;
                        httpListener.Start();

                        Logger.Info($"WebDAV server running at {webdavHost}:{webdavPort}");

                        // Start dispatching requests
                        var t = DispatchHttpRequestsAsync(httpListener, CancellationTokenSource.Token, options.MaxThreadCount);
                        t.Wait(CancellationTokenSource.Token);

                        //do not use console input - it uses 100% CPU when running mono-service in ubuntu
                    }
                    finally
                    {
                        CancellationTokenSource.Cancel();
                        httpListener.Stop();
                    }
                    return 0;

                },
                errors => 1);

            if (exitCode > 0) Environment.Exit(exitCode);
        }


        private static async Task DispatchHttpRequestsAsync(HttpListener httpListener, CancellationToken cancellationToken, int maxThreadCount = Int32.MaxValue)
        {
            // Create a request handler factory that uses basic authentication
            var requestHandlerFactory = new WebDavMailRu.CloudStore.Mailru.RequestHandlerFactory();

            // Create WebDAV dispatcher
            var homeFolder = new MailruStore();
            var webDavDispatcher = new WebDavDispatcher(homeFolder, requestHandlerFactory);

            try
            {
                using (var sem = new SemaphoreSlim(maxThreadCount))
                {
                    var semclo = sem;
                    while (!cancellationToken.IsCancellationRequested)
                    {
                        var httpListenerContext = await httpListener.GetContextAsync().ConfigureAwait(false);
                        if (httpListenerContext == null)
                            break;

                        HttpListenerBasicIdentity identity = (HttpListenerBasicIdentity)httpListenerContext.User.Identity;
                        IHttpContext httpContext = new HttpBasicContext(httpListenerContext, i => i.Name == identity.Name && i.Password == identity.Password);

                        await semclo.WaitAsync(cancellationToken);

                        var task = Task.Run(async () =>
                        {
                            try
                            {
                                await webDavDispatcher.DispatchRequestAsync(httpContext)
                                    .ConfigureAwait(false);
                            }
                            finally
                            {
                                semclo.Release();
                            }

                        }, cancellationToken);
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Error("Global exception", e);
            }



        }


        private static void ShowInfo(CommandLineOptions options)
        {
            string title = GetAssemblyAttribute<AssemblyProductAttribute>(a => a.Product);
            string description = GetAssemblyAttribute<AssemblyDescriptionAttribute>(a => a.Description);
            string copyright = GetAssemblyAttribute<AssemblyCopyrightAttribute>(a => a.Copyright);
            string version = Assembly.GetExecutingAssembly().GetName().Version.ToString();

            System.Console.WriteLine($"  {title}: {description}");
            System.Console.WriteLine($"  v.{version}");
            System.Console.WriteLine($"  {copyright}");

            Logger.Info($"OS Version: {Environment.OSVersion}");
            Logger.Info($"CLR: {GetFrameworkDescription()}");
            Logger.Info($"User interactive: {Environment.UserInteractive}");
            Logger.Info($"Version: {version}");
            Logger.Info($"Max threads count: {options.MaxThreadCount}");
        }

        private static string GetAssemblyAttribute<T>(Func<T, string> value) where T : Attribute
        {
            T attribute = (T)Attribute.GetCustomAttribute(Assembly.GetExecutingAssembly(), typeof(T));
            return null == attribute ? null : value.Invoke(attribute);
        }


        private static string GetFrameworkDescription()
        {
            // detect .NET Core
            var assembly = typeof(System.Runtime.GCSettings).GetTypeInfo().Assembly;
            var assemblyPath = assembly.CodeBase.Split(new[] { '/', '\\' }, StringSplitOptions.RemoveEmptyEntries);
            int netCoreAppIndex = Array.IndexOf(assemblyPath, "Microsoft.NETCore.App");
            if (netCoreAppIndex > 0 && netCoreAppIndex < assemblyPath.Length - 2)
                return ".NET Core " + assemblyPath[netCoreAppIndex + 1];

            // detect .NET Mono
            Type type = Type.GetType("Mono.Runtime");
            if (type != null)
            {
                MethodInfo displayName = type.GetMethod("GetDisplayName", BindingFlags.NonPublic | BindingFlags.Static);
                return displayName != null
                    ? "Mono " + displayName.Invoke(null, null)
                    : "unknown";
            }

            // .NET Framework, yep?
            return ".NET Framework " + Environment.Version;
        }
    }
}
