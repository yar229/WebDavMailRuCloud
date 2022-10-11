using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using YaR.Clouds.Common;

namespace YaR.Clouds.Base
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

        public File(string fullPath, long size, IFileHash hash = null)
        {
            FullPath = fullPath;
            ServiceInfo = FilenameServiceInfo.Parse(WebDavPath.Name(fullPath));

            _originalSize = size;
            _hash = hash;
        }

        public File(string fullPath, long size, params PublicLinkInfo[] links) 
            : this(fullPath, size)
        {
            PublicLinks.AddRange(links);
        }

        private IFileHash _hash;

        /// <summary>
        /// makes copy of this file with new path
        /// </summary>
        /// <param name="newfullPath"></param>
        /// <returns></returns>
        public virtual File New(string newfullPath)
        {
            var file =  new File(newfullPath, Size, Hash)
            {
                CreationTimeUtc = CreationTimeUtc,
                LastAccessTimeUtc = LastAccessTimeUtc,
                LastWriteTimeUtc = LastWriteTimeUtc
            };
            file.PublicLinks.AddRange(PublicLinks);

            return file;
        }

        /// <summary>
        /// Gets file name.
        /// </summary>
        /// <value>File name.</value>
        public string Name
        {
            get => _name;
            private set
            {
                _name = value;

                Extension = System.IO.Path.GetExtension(_name)?.TrimStart('.') ?? string.Empty;
            }
        } //WebDavPath.Name(FullPath)
        private string _name;

        /// <summary>
        /// Gets file extension (without ".")
        /// </summary>
        public string Extension { get; private set; }

        /// <summary>
        /// Gets file hash value.
        /// </summary>
        /// <value>File hash.</value>
        public virtual IFileHash Hash
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

        protected virtual File FileHeader => null;

        /// <summary>
        /// Gets full file path with name on server.
        /// </summary>
        /// <value>Full file path.</value>
        public string FullPath
        {
            get => _fullPath;
            protected set
            {
                _fullPath = WebDavPath.Clean(value);
                Name = WebDavPath.Name(_fullPath);
            }
        }

        private string _fullPath;

        /// <summary>
        /// Path to file (without filename)
        /// </summary>
        public string Path => WebDavPath.Parent(FullPath);

        /// <summary>
        /// Gets public file link.
        /// </summary>
        /// <value>Public link.</value>
        public List<PublicLinkInfo> PublicLinks => _publicLinks ??= new List<PublicLinkInfo>();

        private List<PublicLinkInfo> _publicLinks;

        public IEnumerable<PublicLinkInfo> GetPublicLinks(Cloud cloud)
        {
            return !PublicLinks.Any() 
                ? cloud.GetSharedLinks(FullPath) 
                : PublicLinks;
        }

        /// <summary>
        /// List of phisical files contains data
        /// </summary>
        public virtual List<File> Parts => new() {this};
        public virtual IList<File> Files => new List<File> { this };

        private static readonly DateTime MinFileDate = new(1900, 1, 1);
        public virtual DateTime CreationTimeUtc { get; set; } = MinFileDate;
        public virtual DateTime LastWriteTimeUtc { get; set; } = MinFileDate;
        public virtual DateTime LastAccessTimeUtc { get; set; } = MinFileDate;

        public FileAttributes Attributes { get; set; } = FileAttributes.Normal;

        public bool IsFile => true;
        public FilenameServiceInfo ServiceInfo { get; protected set; }

        //TODO : refact, bad design
        public void SetName(string destinationName)
        {
            FullPath = WebDavPath.Combine(Path, destinationName);
            if (ServiceInfo != null) ServiceInfo.CleanName = Name;

            if (Files.Count <= 1) 
                return;

            string path = Path;
            foreach (var fiFile in Parts)
                fiFile.FullPath = WebDavPath.Combine(path, destinationName + fiFile.ServiceInfo.ToString(false)); //TODO: refact
        }

        //TODO : refact, bad design
        public void SetPath(string fullPath)
        {
            FullPath = WebDavPath.Combine(fullPath, Name);
            if (Parts.Count <= 1) 
                return;

            foreach (var fiFile in Parts)
                fiFile.FullPath = WebDavPath.Combine(fullPath, fiFile.Name); //TODO: refact
        }


        //TODO : refact, bad design
        public CryptoKeyInfo EnsurePublicKey(Cloud cloud)
        {
            if (!ServiceInfo.IsCrypted || null != ServiceInfo.CryptInfo.PublicKey)
                return ServiceInfo.CryptInfo.PublicKey;

            var info = cloud.DownloadFileAsJson<HeaderFileContent>(FileHeader ?? this);
            ServiceInfo.CryptInfo.PublicKey = info.PublicKey;
            return ServiceInfo.CryptInfo.PublicKey;
        }

        public PublishInfo ToPublishInfo(Cloud cloud, bool generateDirectVideoLink, SharedVideoResolution videoResolution)
        {
            var info = new PublishInfo();

            bool isSplitted = Files.Count > 1;

            int cnt = 0;
            foreach (var innerFile in Files)
            {
                if (innerFile.PublicLinks.Any())
                    info.Items.Add(new PublishInfoItem
                    {
                        Path = innerFile.FullPath,
                        Urls =  innerFile.PublicLinks.Select(pli => pli.Uri).ToList(),
                        PlaylistUrl = !isSplitted || cnt > 0
                                          ? generateDirectVideoLink 
                                                ? ConvertToVideoLink(cloud, innerFile.PublicLinks.First().Uri, videoResolution)
                                                : null
                                          : null
                    });
                cnt++;
            }

            return info;
        }

        private static string ConvertToVideoLink(Cloud cloud, Uri publicLink, SharedVideoResolution videoResolution)
        {
            return cloud.Account.RequestRepo.ConvertToVideoLink(publicLink, videoResolution);
                       
                       
                   //    GetShardInfo(ShardType.WeblinkVideo).Result.Url +
                   //videoResolution.ToEnumMemberValue() + "/" + //"0p/" +
                   //Base64Encode(publicLink.TrimStart('/')) +
                   //".m3u8?double_encode=1";
        }


    }
}

