using System;
using System.IO;
using System.Security.Principal;
using System.Threading;
using WebDAVSharp.Server.Stores.BaseClasses;


namespace WebDAVSharp.Server.Stores.DiskStore
{
    /// <summary>
    /// This class implements a disk-based 
    /// <see cref="IWebDavStoreItem" /> which can be either
    /// a folder on disk (
    /// <see cref="WebDavDiskStoreCollection" />) or a file on disk
    /// (
    /// <see cref="WebDavDiskStoreDocument" />).
    /// </summary>
    public class WebDavDiskStoreItem : WebDavStoreItemBase
    {

        #region Variables

        /// <summary>
        /// Gets the Identity of the person logged on via HTTP Request.
        /// </summary>
        protected readonly WindowsIdentity Identity;

        private readonly string _path;

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="WebDavDiskStoreItem" /> class.
        /// </summary>
        /// <param name="parentCollection">The parent 
        /// <see cref="WebDavDiskStoreCollection" /> that contains this 
        /// <see cref="WebDavDiskStoreItem" />;
        /// or 
        /// <c>null</c> if this is the root 
        /// <see cref="WebDavDiskStoreCollection" />.</param>
        /// <param name="path">The path that this <see cref="WebDavDiskStoreItem" /> maps to.</param>
        /// <exception cref="System.ArgumentNullException">path</exception>
        /// <exception cref="ArgumentNullException"><paramref name="path" /> is <c>null</c> or empty.</exception>
        protected WebDavDiskStoreItem(WebDavDiskStoreCollection parentCollection, string path)
            : base(parentCollection, path)
        {
            if (String.IsNullOrWhiteSpace(path))
                throw new ArgumentNullException("path");
            _path = path;

            Identity = (WindowsIdentity)Thread.GetData(Thread.GetNamedDataSlot(WebDavServer.HttpUser));
            
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the path to this <see cref="WebDavDiskStoreItem" />.
        /// </summary>
        public override string ItemPath
        {
            get
            {
                return _path;
            }
        }

        /// <summary>
        /// Gets or sets the name of this <see cref="IWebDavStoreItem" />.
        /// </summary>
        /// <exception cref="System.InvalidOperationException">Unable to rename item</exception>
        public new string Name
        {
            get
            {
                return Path.GetFileName(_path);
            }

            set
            {
                throw new InvalidOperationException("Unable to rename item");
            }
        }

        /// <summary>
        /// Gets if this <see cref="IWebDavStoreItem" /> is a collection.
        /// </summary>
        public new bool IsCollection
        {
            get
            {
                // get the file attributes for file or directory
                FileAttributes attr = File.GetAttributes(_path);

                //detect whether its a directory or file
                return (attr & FileAttributes.Directory) == FileAttributes.Directory;
            }
        }

        /// <summary>
        /// Gets the creation date of this <see cref="IWebDavStoreItem" />.
        /// </summary>
        public override DateTime CreationDate
        {
            get
            {
                // get the file attributes for file or directory
                return File.GetCreationTime(_path);
            }
        }

        /// <summary>
        /// Gets the modification date of this <see cref="IWebDavStoreItem" />.
        /// </summary>
        public override DateTime ModificationDate
        {
            get
            {
                // get the file attributes for file or directory
                return File.GetLastWriteTime(_path);
            }
        }

        /// <summary>
        /// Gets the hidden state of this <see cref="IWebDavStoreItem" />.
        /// </summary>
        /// <remarks>
        /// Source: <see href="http://stackoverflow.com/questions/3612035/c-sharp-check-if-a-directory-is-hidden" />
        /// </remarks>
        public new int Hidden
        {
            get
            {
                DirectoryInfo dir = new DirectoryInfo(_path);
                return (dir.Attributes & FileAttributes.Hidden) != 0 ? 1 : 0;
            }
        }

        #endregion


        
    }
}