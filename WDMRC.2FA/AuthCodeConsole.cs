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

        public TwoFaCodeResult Get(string login, bool isAutoRelogin)
        {
            Console.Write($"Auth code for {login} required {(isAutoRelogin ? "(auto relogin)" : string.Empty)}:");
            string code = Console.ReadLine();

            Console.Write($"Remember this device? (y/n, 1/0, true/false): ");
            string strRemember = Console.ReadLine();
            bool doRemember = strRemember != null && (PositiveAnswers.Contains(strRemember.ToUpper()));

            return new TwoFaCodeResult
            {
                Code = code,
                DoNotAskAgainForThisDevice = doRemember
            };
        }

        private static readonly string[] PositiveAnswers = {"Y", "1", "Д", "ДА", "T", "TRUE"};
    }
}
