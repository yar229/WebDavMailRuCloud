using System;
using CommandLine;

namespace YaR.WebDavMailRu
{
    class CommandLineOptions
    {
        [Option('p', "port", Required = true, HelpText = "WebDAV server port")]
        public int Port { get; set; }

        [Obsolete]
        [Option('l', "login", Required = false, HelpText = "Login to Mail.ru Cloud", Hidden = true)]
        public string Login { get; set; }

        [Obsolete]
        [Option('s', "password", Required = false, HelpText = "Password to Mail.ru Cloud", Hidden = true)]
        public string Password { get; set; }

        [Option("maxthreads", Default = 5, HelpText = "Maximum concurrent connections to cloud.mail.ru")]
        public int MaxThreadCount { get; set; }

        [Option("user-agent", HelpText = "\"browser\" user-agent")]
        public string UserAgent { get; set; }


    }
}
