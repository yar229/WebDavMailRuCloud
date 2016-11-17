using CommandLine;

namespace YaR.WebDavMailRu
{
    class CommandLineOptions
    {
        [Option('p', "port", Required = true, HelpText = "WebDAV server port")]
        public int Port { get; set; }

        [Option('l', "login", Required = true, HelpText = "Login to Mail.ru Cloud")]
        public string Login { get; set; }

        [Option('s', "password", Required = true, HelpText = "Password to Mail.ru Cloud")]
        public string Password { get; set; }

        [Option("maxthreads", Default = 5, HelpText = "Maximum concurrent connections to cloud.mail.ru")]
        public int MaxThreadCount { get; set; }

    }
}
