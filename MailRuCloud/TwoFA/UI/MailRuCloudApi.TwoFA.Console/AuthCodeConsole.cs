using YaR.MailRuCloud.Api;

namespace YaR.MailRuCloud.TwoFA.UI
{
    public class AuthCodeConsole : ITwoFaHandler
    {
        public AuthCodeConsole()
        { }

        public string Get(string login, bool isAutoRelogin)
        {
            System.Console.Write($"Auth code for {login} required {(isAutoRelogin ? "(auto relogin)" : string.Empty)}:");
            return System.Console.ReadLine();
        }
    }
}
