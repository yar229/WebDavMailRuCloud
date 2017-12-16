using System;
using DokanNet;

namespace WDMRC.Dokan.Console
{
    class Program
    {
        static void Main(string[] args)
        {
            var drive = new MailRuCloudDrive(args[0], args[1]);
            drive.Mount(args[2]);
        }
    }
}
