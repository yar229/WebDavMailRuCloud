using System;
using System.Collections.Generic;
using System.Linq;
using YaR.MailRuCloud.Api.Base;

namespace YaR.MailRuCloud.Api.PathResolve
{
    public class LinkManager : LinkManagerBase, IApiPlugin
    {
        public LinkManager(CloudApi api) : base(api)
        {
        }

        public void Register(MailRuCloud cloud)
        {
            cloud.FileUploaded += FileUploaded;
            cloud.LinkRequired = AsRelationalWebLink;
            cloud.FolderModyfy += OnCloudOnFolderModyfy;
            cloud.BeforeItemRemove += OnCloudOnBeforeItemRemove;
            cloud.LinkItema += Add;
            cloud.ItemRenamed += ProcessRename;
        }

        private void OnCloudOnBeforeItemRemove(string fullPath)
        {
            string link = AsRelationalWebLink(fullPath);

            if (!string.IsNullOrEmpty(link))
            {
                //if folder is linked - do not delete inner files/folders if client deleting recursively
                //just try to unlink folder
                RemoveItem(fullPath);
            }
        }

        private void OnCloudOnFolderModyfy(Entry entry)
        {
            var flinks = GetItems(entry.FullPath);
            if (flinks.Any())
            {
                foreach (var flink in flinks)
                {
                    string linkpath = WebDavPath.Combine(entry.FullPath, flink.Name);

                    if (!flink.IsFile)
                        entry.Folders.Add(new Folder(0, 0, 0, linkpath)
                        {
                            CreationTimeUtc = flink.CreationDate ?? DateTime.MinValue
                        });
                    else
                    {
                        if (entry.Files.All(inf => inf.FullPath != linkpath))
                            entry.Files.Add(new File(linkpath, flink.Size, string.Empty));
                    }
                }
            }
        }

        private void FileUploaded(IEnumerable<File> fileParts)
        {
            var file = fileParts?.FirstOrDefault();
            if (null == file) return;

            if (file.Path == "/" && file.Name == LinkContainerName)
                Load();
        }


    }
}