using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using YaR.Clouds.Base.Repos;
using YaR.Clouds.Common;

namespace YaR.Clouds.Base
{
    /// <summary>
    /// Server file info.
    /// </summary>
    [DebuggerDisplay("{" + nameof(FullPath) + "}")]
    public class Folder : IEntry, ICanForget
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Folder" /> class.
        /// </summary>
        public Folder(string fullPath)
        {
            FullPath = WebDavPath.Clean(fullPath);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Folder" /> class.
        /// </summary>
        /// <param name="size">Folder size.</param>
        /// <param name="fullPath">Full folder path.</param>
        /// <param name="publicLinks">Public folder link.</param>
        public Folder(FileSize size, string fullPath, IEnumerable<PublicLinkInfo> publicLinks = null):this(fullPath)
        {
            Size = size;
            if (null != publicLinks)
                PublicLinks.AddRange(publicLinks);
        }

        public IEnumerable<IEntry> Entries
        {
            get
            {
                foreach (var file in Files)
                    yield return file;
                foreach (var folder in Folders)
                    yield return folder;
            }
        }

        public List<File> Files { get; set; } = new List<File>();

        public List<Folder> Folders { get; set; } = new List<Folder>();


        /// <summary>
        /// Gets number of folders in folder.
        /// </summary>
        /// <value>Number of folders.</value>
        public int NumberOfFolders => Entries.OfType<Folder>().Count();

        /// <summary>
        /// Gets number of files in folder.
        /// </summary>
        /// <value>Number of files.</value>
        public int NumberOfFiles => Entries.OfType<File>().Count();

        /// <summary>
        /// Gets folder name.
        /// </summary>
        /// <value>Folder name.</value>
        public string Name => WebDavPath.Name(FullPath);

        /// <summary>
        /// Gets folder size.
        /// </summary>
        /// <value>Folder size.</value>
        public FileSize Size { get; }

        /// <summary>
        /// Gets full folder path on the server.
        /// </summary>
        /// <value>Full folder path.</value>
        public string FullPath
        {
            get;
        }


        /// <summary>
        /// Gets public file link.
        /// </summary>
        /// <value>Public link.</value>
        public List<PublicLinkInfo> PublicLinks => _publicLinks ??= new List<PublicLinkInfo>();
        private List<PublicLinkInfo> _publicLinks;

        public IEnumerable<PublicLinkInfo> GetPublicLinks(Cloud cloud)
        {
            if (!PublicLinks.Any())
                return cloud.GetSharedLinks(FullPath);

            return PublicLinks;
        }


        public DateTime CreationTimeUtc { get; set; } = DateTime.Now.AddDays(-1);

        public DateTime LastWriteTimeUtc { get; set; } = DateTime.Now.AddDays(-1);


        public DateTime LastAccessTimeUtc { get; set; } = DateTime.Now.AddDays(-1);


        public FileAttributes Attributes { get; set; } = FileAttributes.Directory;

        public bool IsFile => false;

		public bool IsChildsLoaded { get; internal set; }


        public int? ServerFoldersCount { get; set; }
        public int? ServerFilesCount { get; set; }

        public PublishInfo ToPublishInfo()
        {
            var info = new PublishInfo();
            if (PublicLinks.Any())
                info.Items.Add(new PublishInfoItem { Path = FullPath, Urls = PublicLinks.Select(pli => pli.Uri).ToList() });
            return info;
        }


	    //public List<KeyValuePair<string, IEntry>> GetLinearChilds()
	    //{
		    
	    //}
        public void Forget(object whomKey)
        {
            string key = whomKey.ToString();
            if (string.IsNullOrEmpty(key))
                return;
            var file = Files.FirstOrDefault(f => f.FullPath == key);
            if (null != file)
                Files.Remove(file);
            else
            {
                var folder = Folders.FirstOrDefault(f => f.FullPath == key);
                if (null != folder)
                    Folders.Remove(folder);
            }
        }
    }
}
