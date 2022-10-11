using System.Collections.Generic;

namespace YaR.Clouds.MailRuCloud.TwoFA.UI
{
    // ReSharper disable once UnusedType.Global
    public class AuthCodeConsole : ITwoFaHandler
    {
        public AuthCodeConsole(IEnumerable<KeyValuePair<string, string>> parames)
        {
        }

        public string Get(string login, bool isAutoRelogin)
        {
            System.Console.Write($"Auth code for {login} required {(isAutoRelogin ? "(auto relogin)" : string.Empty)}:");
            return System.Console.ReadLine();
        }
    }
}
