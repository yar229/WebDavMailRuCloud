using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using CommandLine;
using YaR.Clouds.Base;

namespace YaR.Clouds.Console
{
    // ReSharper disable once ClassNeverInstantiated.Global
    [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
    class CommandLineOptions
    {
        [Option('p', "port", Separator = ',', Required = false, Default = new[]{801}, HelpText = "WebDAV server port")]
        public IEnumerable<int> Port { get; set; }

        [Option('h', "host", Required = false, Default = "http://127.0.0.1", HelpText = "WebDAV server host, including protocol")]
        public string Host { get; set; }

        [Obsolete]
        [Option('l', "login", Required = false, HelpText = "Login to Mail.ru Cloud", Hidden = true)]
        // ReSharper disable once UnusedMember.Global
        public string Login { get; set; }

        [Obsolete]
        [Option('s', "password", Required = false, HelpText = "Password to Mail.ru Cloud", Hidden = true)]
        // ReSharper disable once UnusedMember.Global
        public string Password { get; set; }

        [Option("maxthreads", Default = 5, HelpText = "Maximum concurrent connections to cloud")]
        public int MaxThreadCount { get; set; }

        [Option("user-agent", HelpText = "\"browser\" user-agent")]
        public string UserAgent { get; set; }

        [Option("sec-ch-ua", HelpText = "\"browser\" sec-ch-ua")]
        public string SecChUa { get; set; }

        [Option("install", Required = false, HelpText = "install as Windows service with name")]
        public string ServiceInstall { get; set; }

        [Option("install-display", Required = false, HelpText = "display name for Windows service")]
        public string ServiceInstallDisplayName { get; set; }

        [Option("uninstall", Required = false, HelpText = "uninstall Windows service")]
        public string ServiceUninstall { get; set; }

        [Option("service", Required = false, Default = false, HelpText = "Started as a service")]
        public bool ServiceRun { get; set; }

        [Option("protocol", Default = Protocol.WebM1Bin, HelpText = "Cloud protocol")]
        public Protocol Protocol { get; set; }

        [Option("cache-listing", Default = 30, HelpText = "Cache folders listing, sec")]
        public int CacheListingSec { get; set; }

		[Option("cache-listing-depth", Default = 1, HelpText = "List query folder depth, always equals 1 when cache-listing>0")]
		public int CacheListingDepth { get; set; }

        [Option("proxy-address", Default = "", HelpText = "Proxy address i.e. http://192.168.1.1:8080")]
        public string ProxyAddress { get; set; }
        [Option("proxy-user", Default = "", HelpText = "Proxy user")]
        public string ProxyUser { get; set; }
        [Option("proxy-password", Default = "", HelpText = "Proxy password")]
        public string ProxyPassword { get; set; }

        [Option("use-locks", Required = false, Default = false, HelpText = "locking feature")]
        public bool UseLocks { get; set; }

        [Option("use-deduplicate", Required = false, Default = false, HelpText = "Use cloud deduplicate feature to minimize traffic")]
        public bool UseDeduplicate { get; set; }
    }
}
