using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using YaR.MailRuCloud.Api.Base;
using File = YaR.MailRuCloud.Api.Base.File;

namespace YaR.MailRuCloud.Api.PathResolve
{
    public class SharedContainer : IApiPlugin
    {
        public void Register(MailRuCloud cloud)
        {
            cloud.FolderListed += entry =>
            {
                if (entry.FullPath == WebDavPath.Root)
                    entry.Files.Add(new File("/item.shared.wdmrc", 0, string.Empty));
            };

            cloud.BeforeFileDownload += (IEnumerable<File> file, out Stream stream) =>
            {
                stream = null;
                var files = file.ToList();
                if (files.Count == 1 && files[0].FullPath == "/item.shared.wdmrc")
                {
                    stream = new MemoryStream(Encoding.UTF8.GetBytes("test data"));
                }
                
            };
        }
    }
}