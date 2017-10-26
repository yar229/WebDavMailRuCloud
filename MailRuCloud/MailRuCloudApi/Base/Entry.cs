//-----------------------------------------------------------------------
// <created file="Entry.cs">
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
    /// List of items in cloud.
    /// </summary>
    [DebuggerDisplay("{" + nameof(FullPath) + "}")]
    public class Entry
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Entry" /> class.
        /// </summary>
        /// <param name="folders">List of the folders.</param>
        /// <param name="files">List of the files.</param>
        /// <param name="path">The entry path on the server.</param>
        public Entry(IList<Folder> folders, IList<File> files, string path)
        {
            Folders = folders ?? new List<Folder>();
            Files = files ?? new List<File>();

            NumberOfFolders = folders?.Count ?? 0;
            NumberOfFiles = files?.Count ?? 0;
            FullPath = path;
        }

        /// <summary>
        /// Gets number of the folders.
        /// </summary>
        public int NumberOfFolders { get; }

        /// <summary>
        /// Gets number of the files.
        /// </summary>
        public int NumberOfFiles { get; }

        public int NumberOfItems => NumberOfFolders + NumberOfFiles;

        /// <summary>
        /// Gets list of the folders with their specification.
        /// </summary>
        public IList<Folder> Folders { get; }

        /// <summary>
        /// Gets list of the files with their specification.
        /// </summary>
        public IList<File> Files { get; }

        /// <summary>
        /// Gets full entry path on the server.
        /// </summary>
        public string FullPath { get; }

        public long Size { get; set; }
        public string WebLink { get; set; }
        public bool IsFile { get; set; }
        public string Name { get; set; }
        public DateTime? CreationDate { get; set; }
    }
}
