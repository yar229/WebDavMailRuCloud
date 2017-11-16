//-----------------------------------------------------------------------
// <created file="File.cs">
//     Mail.ru cloud client created in 2016.
// </created>
// <author>Korolev Erast.</author>
//-----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;

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
            _size = size;
            _hash = hash;

            ServiceInfo = FilenameServiceInfo.Parse(WebDavPath.Name(fullPath));
        }


        private string _fullPath;
        private FileSize _size;
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
        public virtual FileSize Size
        {
            get => _size;
            set => _size = value;
        }

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

        /// <summary>
        /// If file splitted to several phisical files
        /// </summary>
        public bool IsSplitted => Parts.Any(f => f.FullPath != FullPath);

        public bool IsFile => true;
        public FilenameServiceInfo ServiceInfo { get; protected set; }

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

        public void SetPath(string fullPath)
        {
            FullPath = WebDavPath.Combine(fullPath, Name);
            if (Parts.Count > 1)
                foreach (var fiFile in Parts)
                {
                    fiFile.FullPath = WebDavPath.Combine(fullPath, fiFile.Name); //TODO: refact
                }
        }
    }

    public class FileSplitInfo
    {
        public bool IsHeader { get; set; }
        public bool IsPart => PartNumber > 0;
        public int PartNumber { get; set; }
    }

    public class FilenameServiceInfo
    {
        public string CleanName { get; set; }

        public bool IsCrypted => CryptInfo != null;
        public CryptInfo CryptInfo { get; set; }

        public bool IsSplitted => SplitInfo != null;
        public FileSplitInfo SplitInfo { get; set; }

        public override string ToString()
        {
            return ".wdmrc." + SplitInfo?.PartNumber.ToString("D3") ?? "000" + CryptInfo?.AlignBytes.ToString("x") ?? string.Empty;
        }

        public string ToString(bool withName)
        {
            return withName
                ? CleanName + ToString()
                : ToString();
        }

        public static FilenameServiceInfo Parse(string filename)
        {
            var res = new FilenameServiceInfo();

            var m = Regex.Match(filename, @"\A(?<cleanname>.*?)(\.wdmrc\.(?<partnumber>\d\d\d)(?<align>[0-9a-f])?)?\Z", RegexOptions.Compiled);
            if (!m.Success)
                throw new InvalidOperationException("Cannot parse filename");

            res.CleanName = m.Groups["cleanname"].Value;

            string partnumber = m.Groups["partnumber"].Value;
            res.SplitInfo = new FileSplitInfo
            {
                IsHeader = string.IsNullOrEmpty(partnumber),
                PartNumber = string.IsNullOrEmpty(partnumber) ? 0 : int.Parse(m.Groups["partnumber"].Value)
            };

            string align = m.Groups["align"].Value;
            if (!string.IsNullOrEmpty(align))
            {
                res.CryptInfo = new CryptInfo
                {
                    AlignBytes = Convert.ToUInt32(align, 16)
                };
            }

            return res;
        }
    }
}

