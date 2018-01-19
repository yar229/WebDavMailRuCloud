using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using YaR.MailRuCloud.Api.Base;
using File = YaR.MailRuCloud.Api.Base.File;

namespace YaR.CloudMailRu.Client.Console
{
    static class DecryptStub
    {
        public static int Decrypt(DecryptOptions cmdoptions)
        {
            var cryFiles = new DirectoryInfo(cmdoptions.Source)
                .GetFiles()
                .Select(fi => new File(fi.FullName, fi.Length, string.Empty))
                .ToGroupedFiles()
                .Where(f => f.ServiceInfo.IsCrypted)
                .ToList();

            return 0;
        }


        private static IEnumerable<File> ToGroupedFiles(this IEnumerable<File> list)
        {
            var groupedFiles = list
                .GroupBy(f => f.ServiceInfo.CleanName, file => file)
                .SelectMany(group => group.Count() == 1
                    ? group.Take(1)
                    : group.Any(f => f.Name == f.ServiceInfo.CleanName)
                        ? Enumerable.Repeat(new SplittedFile(group.ToList()), 1)
                        : group.Select(file => file));

            return groupedFiles;
        }
    }
}