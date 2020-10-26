using System.IO;
using System.Text.RegularExpressions;
using YaR.Clouds;
using YaR.Clouds.Base;
using YaR.Clouds.Base.Repos.MailRuCloud;

namespace YaR.CloudMailRu.Client.Console
{
    static class UploadStub
    {
        public static int Upload(UploadOptions cmdoptions)
        {
            string user = cmdoptions.Login;
            string password = cmdoptions.Password;
            string listname = cmdoptions.FileList;
            string targetpath = cmdoptions.Target;



            if (targetpath.StartsWith(@"\\\"))
                targetpath = Regex.Replace(targetpath, @"\A\\\\\\.*?\\.*?\\", @"\");

            targetpath = WebDavPath.Clean(targetpath);

            var settings = new CloudSettings
            {
                TwoFaHandler = null,
                Protocol = Protocol.WebM1Bin
            };
            var credentials = new Credentials(user, password);
            var cloud = new Cloud(settings, credentials);

            using (var file = new StreamReader(listname))
            {
                string line;
                while ((line = file.ReadLine()) != null)
                {
                    System.Console.WriteLine($"Source: {line}");
                    var fileInfo = new FileInfo(line);

                    string targetfile = WebDavPath.Combine(targetpath, fileInfo.Name);
                    System.Console.WriteLine($"Target: {targetfile}");

                    using (var source = System.IO.File.Open(line, FileMode.Open, FileAccess.Read, FileShare.Read))
                    {
                        var hasher = new MailRuSha1Hash();
                        hasher.Append(source);
                        var hash = hasher.Hash;
                        if (cloud.AddFile(hash, targetfile, fileInfo.Length, ConflictResolver.Rename).Result.Success)
                        {
                            System.Console.WriteLine("Added by hash");
                        }
                        else
                        {
                            source.Seek(0, SeekOrigin.Begin);
                            var buffer = new byte[64000];
                            long wrote = 0;
                            using (var target = cloud.GetFileUploadStream(WebDavPath.Combine(targetfile, fileInfo.Name), fileInfo.Length, null, null).Result)
                            {
                                int read;
                                while ((read = source.Read(buffer, 0, buffer.Length)) > 0)
                                {
                                    target.Write(buffer, 0, read);
                                    wrote += read;
                                    System.Console.Write($"\r{wrote / fileInfo.Length * 100}%");
                                }
                            }
                        }

                        System.Console.WriteLine(" Done.");
                        //source.CopyTo(target);
                    }
                }
            }

            return 0;
        }
    }
}
