//-----------------------------------------------------------------------
// <created file="Folder.cs">
//     Mail.ru cloud client created in 2016.
// </created>
// <author>Korolev Erast.</author>
//-----------------------------------------------------------------------

namespace MailRuCloudApi
{
    /// <summary>
    /// Server file info.
    /// </summary>
    public class Folder
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Folder" /> class.
        /// </summary>
        public Folder()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Folder" /> class.
        /// </summary>
        /// <param name="foldersCount">Number of folders.</param>
        /// <param name="filesCount">Number of files.</param>
        /// <param name="name">Folder name.</param>
        /// <param name="size">Folder size.</param>
        /// <param name="fullPath">Full folder path.</param>
        /// <param name="publicLink">Public folder link.</param>
        public Folder(int foldersCount, int filesCount, string name, FileSize size, string fullPath, string publicLink = null)
        {
            this.NumberOfFolders = foldersCount;
            this.NumberOfFiles = filesCount;
            this.Name = name;
            this.Size = size;
            this.FulPath = fullPath;
            this.PublicLink = publicLink;
        }

        /// <summary>
        /// Gets number of folders in folder.
        /// </summary>
        /// <value>Number of folders.</value>
        public int NumberOfFolders { get; internal set; }

        /// <summary>
        /// Gets number of files in folder.
        /// </summary>
        /// <value>Number of files.</value>
        public int NumberOfFiles { get; internal set; }

        /// <summary>
        /// Gets folder name.
        /// </summary>
        /// <value>Folder name.</value>
        public string Name { get; set; }

        /// <summary>
        /// Gets folder size.
        /// </summary>
        /// <value>Folder size.</value>
        public FileSize Size { get; internal set; }

        /// <summary>
        /// Gets full folder path on the server.
        /// </summary>
        /// <value>Full folder path.</value>
        public string FulPath { get; set; }

        /// <summary>
        /// Gets public folder link.
        /// </summary>
        /// <value>Public link.</value>
        public string PublicLink { get; internal set; }
    }
}
