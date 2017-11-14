using System;
using System.IO;
using System.Text.RegularExpressions;
using YaR.MailRuCloud.Api;
using YaR.MailRuCloud.Api.Base;

namespace WebDAV.Uploader
{
    class Program
    {
        static void Main(string[] args)
        {
            string user = args[0].Trim('"');
            string password = args[1].Trim('"');
            string listname = args[2].Trim('"');
            string targetpath = args[3].Trim('"');



            if (targetpath.StartsWith(@"\\\"))
                targetpath = Regex.Replace(targetpath, @"\A\\\\\\.*?\\.*?\\", @"\");

            targetpath = WebDavPath.Clean(targetpath);

            var cloud = new MailRuCloud(user, password, null);

            using (var file = new StreamReader(listname))
            {
                string line;
                while ((line = file.ReadLine()) != null)
                {
                    Console.WriteLine($"Source: {line}");
                    var fileInfo = new FileInfo(line);

                    string targetfile = WebDavPath.Combine(targetpath, fileInfo.Name);
                    Console.WriteLine($"Target: {targetfile}");

                    using (var source = System.IO.File.Open(line, FileMode.Open, FileAccess.Read, FileShare.Read))
                    using (var target = cloud.GetFileUploadStream(WebDavPath.Combine(targetfile, fileInfo.Name), fileInfo.Length))
                    {
                        var buffer = new byte[64000];
                        int read;
                        long wrote = 0;
                        while((read = source.Read(buffer, 0, buffer.Length)) > 0)
                        {
                            target.Write(buffer, 0, read);
                            wrote += read;
                            Console.Write($"\r{wrote / fileInfo.Length * 100}%");
                        }
                        Console.WriteLine(" Done.");
                        //source.CopyTo(target);
                    }
                }
            }
            Console.ReadKey();
        }
    }
}
