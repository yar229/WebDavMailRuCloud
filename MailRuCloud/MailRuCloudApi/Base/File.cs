//-----------------------------------------------------------------------
// <created file="File.cs">
//     Mail.ru cloud client created in 2016.
// </created>
// <author>Korolev Erast.</author>
//-----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace YaR.MailRuCloud.Api.Base
{
    /// <summary>
    /// Server file info.
    /// </summary>
    [DebuggerDisplay("{" + nameof(FullPath) + "}")]
    public class File : IEntry
    {
        protected File()
        {
        }

        public File(string fullPath, long size, string hash = "")
        {
            FullPath = fullPath;
            ServiceInfo = FilenameServiceInfo.Parse(WebDavPath.Name(fullPath));

            _originalSize = size;
            _hash = hash;
        }


        private string _fullPath;
        private string _hash;

        /// <summary>
        /// makes copy of this file with new path
        /// </summary>
        /// <param name="newfullPath"></param>
        /// <returns></returns>
        public virtual File New(string newfullPath)
        {
            return new File(newfullPath, Size, Hash)
            {
                CreationTimeUtc = CreationTimeUtc,
                LastAccessTimeUtc = LastAccessTimeUtc,
                LastWriteTimeUtc = LastWriteTimeUtc,
                PublicLink = PublicLink
            };
        }

        /// <summary>
        /// Gets file name.
        /// </summary>
        /// <value>File name.</value>
        public virtual string Name => WebDavPath.Name(FullPath);

        /// <summary>
        /// Gets file extension
        /// </summary>
        public string Extension => System.IO.Path.GetExtension(Name);

        /// <summary>
        /// Gets file hash value.
        /// </summary>
        /// <value>File hash.</value>
        public virtual string Hash
        {
            get => _hash;
            internal set => _hash = value;
        }

        /// <summary>
        /// Gets file size.
        /// </summary>
        /// <value>File size.</value>
        public virtual FileSize Size => OriginalSize - (ServiceInfo.CryptInfo?.AlignBytes ?? 0);

        public virtual FileSize OriginalSize
        {
            get => _originalSize;
            set => _originalSize = value;
        }
        private FileSize _originalSize;

        protected virtual File FileHeader { get; } = null;

        /// <summary>
        /// Gets full file path with name on server.
        /// </summary>
        /// <value>Full file path.</value>
        public string FullPath
        {
            get => _fullPath;
            protected set => _fullPath = WebDavPath.Clean(value);
        }

        /// <summary>
        /// Path to file (without filename)
        /// </summary>
        public string Path => WebDavPath.Parent(FullPath);

        /// <summary>
        /// Gets public file link.
        /// </summary>
        /// <value>Public link.</value>
        public string PublicLink { get; internal set; }

        /// <summary>
        /// List of phisical files contains data
        /// </summary>
        public virtual List<File> Parts => new List<File> {this};
        public virtual IList<File> Files => new List<File> { this };

        public virtual DateTime CreationTimeUtc { get; set; }
        public virtual DateTime LastWriteTimeUtc { get; set; }
        public virtual DateTime LastAccessTimeUtc { get; set; }

        public bool IsFile => true;
        public FilenameServiceInfo ServiceInfo { get; protected set; }

        //TODO : refact, bad design
        public void SetName(string destinationName)
        {
            FullPath = WebDavPath.Combine(Path, destinationName);
            if (ServiceInfo != null) ServiceInfo.CleanName = Name;

            if (Files.Count > 1)
            {
                string path = Path;
                foreach (var fiFile in Parts)
                {
                    fiFile.FullPath = WebDavPath.Combine(path, destinationName + fiFile.ServiceInfo.ToString(false)); //TODO: refact
                }
            }
        }

        //TODO : refact, bad design
        public void SetPath(string fullPath)
        {
            FullPath = WebDavPath.Combine(fullPath, Name);
            if (Parts.Count > 1)
                foreach (var fiFile in Parts)
                {
                    fiFile.FullPath = WebDavPath.Combine(fullPath, fiFile.Name); //TODO: refact
                }
        }


        //TODO : refact, bad design
        public CryptoKeyInfo EnsurePublicKey(MailRuCloud cloud)
        {
            if (ServiceInfo.IsCrypted && null == ServiceInfo.CryptInfo.PublicKey)
            {
                var info = cloud.DownloadFileAsJson<HeaderFileContent>(FileHeader ?? this);
                ServiceInfo.CryptInfo.PublicKey = info.PublicKey;
            }
            return ServiceInfo.CryptInfo.PublicKey;
        }

        public PublishInfo ToPublishInfo()
        {
            var info = new PublishInfo();
            foreach (var innerFile in Files)
            {
                if (!string.IsNullOrEmpty(innerFile.PublicLink))
                    info.Items.Add(new PublishInfoItem{Path = innerFile.FullPath, Url = ConstSettings.PublishFileLink + innerFile.PublicLink});
            }
            return info;
        }
    }
}

