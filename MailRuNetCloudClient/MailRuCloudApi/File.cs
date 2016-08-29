//-----------------------------------------------------------------------
// <created file="File.cs">
//     Mail.ru cloud client created in 2016.
// </created>
// <author>Korolev Erast.</author>
//-----------------------------------------------------------------------

using System;
using System.ComponentModel;

namespace MailRuCloudApi
{
    /// <summary>
    /// Cloud file type.
    /// </summary>
    public enum FileType
    {
        /// <summary>
        /// File as single file.
        /// </summary>
        SingleFile = 0,

        /// <summary>
        /// File composed from several pieces, this file type has not hash and public link.
        /// </summary>
        MultiFile = 1
    }

    /// <summary>
    /// Server file info.
    /// </summary>
    public class File
    {
        private string _fullPath;
        private string _name;

        /// <summary>
        /// Gets file name.
        /// </summary>
        /// <value>File name.</value>
        public string Name
        {
            get { return _name; }
            set
            {
                if (value.Contains("/") || value.Contains("\\")) throw new InvalidEnumArgumentException(nameof(Name));
                _name = value;
            }
        }

        public string Extension => System.IO.Path.GetExtension(Name);

        /// <summary>
        /// Gets file hash value.
        /// </summary>
        /// <value>File hash.</value>
        public string Hash { get; internal set; }

        /// <summary>
        /// Gets file size.
        /// </summary>
        /// <value>File size.</value>
        public FileSize Size { get; internal set; }

        /// <summary>
        /// Gets full file path with name in server.
        /// </summary>
        /// <value>Full file path.</value>
        public string FullPath
        {
            get
            {
                return _fullPath;
            }
            set
            {
                _fullPath = value.Replace("\\", "/");
                if (!string.IsNullOrEmpty(Name) && !_fullPath.EndsWith("/" + Name)) _fullPath = _fullPath.TrimEnd('/') + "/" + Name;
            }
        }

        public string Path
        {
            get
            {
                int index = FullPath.LastIndexOf(Name, StringComparison.Ordinal);
                string s = index >= 0 ? FullPath.Substring(0, index) : FullPath;
                //if (s.Length > 1 && s.EndsWith("/")) s = s.Remove(s.Length - 1, 1);
                return s;
            }
        }

        /// <summary>
        /// Gets public file link.
        /// </summary>
        /// <value>Public link.</value>
        public string PublicLink { get; internal set; }

        /// <summary>
        /// Gets cloud file type.
        /// </summary>
        public FileType Type { get; internal set; }

        /// <summary>
        /// Gets or sets base file name.
        /// </summary>
        internal string PrimaryName { get; set; }

        /// <summary>
        /// Gets or sets base file size.
        /// </summary>
        /// <value>File size.</value>
        internal FileSize PrimarySize { get; set; }
    }
}
