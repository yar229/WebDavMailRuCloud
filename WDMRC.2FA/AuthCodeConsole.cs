using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MailRuCloudApi.TwoFA
{
    public class AuthCodeConsole : ITwoFaHandler
    {
        public AuthCodeConsole()
        { }

        public string Get(string login, bool isAutoRelogin)
        {
            Console.Write($"Auth code for {login} required {(isAutoRelogin ? "(auto relogin)" : string.Empty)}:");
            return Console.ReadLine();
        }
    }
}
