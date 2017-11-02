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
        }


        private string _fullPath;
        private FileSize _size;
        private string _hash;

        /// <summary>
        /// Gets file name.
        /// </summary>
        /// <value>File name.</value>
        //TODO: refact
        public virtual string Name => WebDavPath.Name(FullPath); //FullPath.Substring(FullPath.LastIndexOf("/", StringComparison.Ordinal) + 1);

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
        /// Gets full file path with name in server.
        /// </summary>
        /// <value>Full file path.</value>
        public string FullPath
        {
            get => _fullPath;
            protected set => _fullPath = WebDavPath.Clean(value);
        }

        public string Path => WebDavPath.Parent(FullPath);

        /// <summary>
        /// Gets public file link.
        /// </summary>
        /// <value>Public link.</value>
        public string PublicLink { get; internal set; }

        /// <summary>
        /// List of physical files
        /// </summary>
        public virtual List<File> Parts => new List<File> {this};

        public virtual DateTime CreationTimeUtc { get; set; }
        public virtual DateTime LastWriteTimeUtc { get; set; }
        public virtual DateTime LastAccessTimeUtc { get; set; }
        public bool IsSplitted => Parts.Any(f => f.FullPath != FullPath);

        public bool IsFile => true;

        public void SetName(string destinationName)
        {
            string path = WebDavPath.Parent(FullPath);
            FullPath = WebDavPath.Combine(path, destinationName);
            if (Parts.Count > 1)
                foreach (var fiFile in Parts)
                {
                    fiFile.FullPath = WebDavPath.Combine(path, destinationName + ".wdmrc" + fiFile.Extension); //TODO: refact
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
        }}
}

