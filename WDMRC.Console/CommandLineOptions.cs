using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommandLine;
using CommandLine.Text;

namespace FooConsole
{
    class CommandLineOptions
    {
        [Option('p', "port", Required = true, HelpText = "WebDAV server port")]
        public int Port { get; set; }

        [Option('l', "login", Required = true, HelpText = "Login to Mail.ru Cloud")]
        public string Login { get; set; }

        [Option('s', "password", Required = true, HelpText = "Password to Mail.ru Cloud")]
        public string Password { get; set; }
    }
}
