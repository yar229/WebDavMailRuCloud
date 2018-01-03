using System;

namespace YaR.MailRuCloud.Fs
{
    static class Program
    {
        static void Main(string[] args)
        {
            Environment.ExitCode = new MrcfsService().Run();
        }
    }
}