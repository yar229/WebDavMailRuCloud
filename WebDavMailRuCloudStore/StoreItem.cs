using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using Common.Logging;
using WebDAVSharp.Server;
using WebDAVSharp.Server.Stores;
using WebDAVSharp.Server.Stores.BaseClasses;


namespace WebDavMailRuCloudStore
{
    /// <summary>
    /// This class implements a disk-based 
    /// <see cref="IWebDavStoreItem" /> which can be either
    /// a folder on disk (
    /// <see cref="WebDavMailRuCloudStoreCollection" />) or a file on disk
    /// (
    /// <see cref="WebDavMailRuCloudStoreDocument" />).
    /// </summary>
    public class WebDavMailRuCloudStoreItem : WebDavStoreItemBase
    {
        /// <summary>
        /// Gets the Identity of the person logged on via HTTP Request.
        /// </summary>
        protected readonly WindowsIdentity Identity;

        /// <summary>
        /// Log
        /// </summary>
        protected ILog Log;
        private readonly WebDavMailRuCloudStoreCollection _parentCollection;
        private readonly string _path;

        /// <summary>
        /// Initializes a new instance of the <see cref="WebDAVSharp.Server.Stores.DiskStore.WebDavDiskStoreItem" /> class.
        /// </summary>
        /// <param name="parentCollection">The parent 
        /// <see cref="WebDavMailRuCloudStoreCollection" /> that contains this 
        /// <see cref="WebDavMailRuCloudStoreItem" />;
        /// or 
        /// <c>null</c> if this is the root 
        /// <see cref="WebDavMailRuCloudStoreCollection" />.</param>
        /// <param name="path">The path that this <see cref="WebDavMailRuCloudStoreItem" /> maps to.</param>
        /// <exception cref="System.ArgumentNullException">path</exception>
        /// <exception cref="ArgumentNullException"><paramref name="path" /> is <c>null</c> or empty.</exception>
        protected WebDavMailRuCloudStoreItem(WebDavMailRuCloudStoreCollection parentCollection, string path) : base(parentCollection, path)
        {
            //if (String.IsNullOrWhiteSpace(path))
            //    throw new ArgumentNullException("path");

            _parentCollection = parentCollection;
            _path = path;
            Identity = (WindowsIdentity)Thread.GetData(Thread.GetNamedDataSlot(WebDavServer.HttpUser));
            Log = LogManager.GetCurrentClassLogger();
        }

        /// <summary>
        /// Gets the path to this <see cref="WebDavMailRuCloudStoreItem" />.
        /// </summary>
        public override string ItemPath => _path;

        #region IWebDAVStoreItem Members

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
        public new virtual bool IsCollection
        {
            get
            {
                // get the file attributes for file or directory
                //FileAttributes attr = File.GetAttributes(_path);

                //detect whether its a directory or file
                //return (attr & FileAttributes.Directory) == FileAttributes.Directory;

                return true; //TODO: !!!
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
                //return File.GetCreationTime(_path);
                return DateTime.MinValue;  //TODO: !!!
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
                //return File.GetLastWriteTime(_path);
                return DateTime.MinValue;  //TODO: !!!
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
                //DirectoryInfo dir = new DirectoryInfo(_path);
                //return (dir.Attributes & FileAttributes.Hidden) != 0 ? 1 : 0;
                return 0;
            }
        }

        #endregion
    }
}
