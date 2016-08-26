using System;
using System.Diagnostics;
using System.IO;
using System.Security.Principal;
using System.Web;
using WebDAVSharp.Server.Exceptions;
using WebDAVSharp.Server.Utilities;

namespace WebDAVSharp.Server.Stores.DiskStore
{
    /// <summary>
    /// This class implements a disk-based <see cref="WebDavDiskStoreDocument" /> mapped to a file.
    /// </summary>
    [DebuggerDisplay("File ({Name})")]
    public sealed class WebDavDiskStoreDocument : WebDavDiskStoreItem, IWebDavStoreDocument
    {
        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="WebDavDiskStoreDocument" /> class.
        /// </summary>
        /// <param name="parentCollection">The parent 
        /// <see cref="WebDavDiskStoreCollection" /> that contains this 
        /// <see cref="WebDavDiskStoreItem" />;
        /// or 
        /// <c>null</c> if this is the root 
        /// <see cref="WebDavDiskStoreCollection" />.</param>
        /// <param name="path">The path that this <see cref="WebDavDiskStoreItem" /> maps to.</param>
        /// <exception cref="ArgumentNullException"><paramref name="path" /> is <c>null</c> or empty.</exception>
        public WebDavDiskStoreDocument(WebDavDiskStoreCollection parentCollection, string path)
            : base(parentCollection, path)
        {
            // Do nothing here
        }

        #endregion

        #region Functions

        /// <summary>
        /// Gets the size of the document in bytes.
        /// </summary>
        public long Size
        {
            get { return new FileInfo(ItemPath).Length; }
        }

        /// <summary>
        /// Gets the mime type of this <see cref="IWebDavStoreItem" />.
        /// </summary>
        public string MimeType
        {
            get
            {
                return MimeMapping.GetMimeMapping(Name);
            }
        }

        /// <summary>
        /// Gets the etag of this <see cref="IWebDavStoreItem" />.
        /// </summary>
        public string Etag
        {
            get
            {
                // itempath is currently the absolute path, if we want to work with multiple servers and load balancing,
                // we need to change this to the relative path
                // For this moment, we can't get access to the root path for calculating the relative path, 
                // because we don't have an instance of WebDAVDiskStore
                return Md5Util.Md5HashStringForUtf8String(ItemPath + ModificationDate + Hidden + Size);
            }
        }

        /// <summary>
        /// Opens a <see cref="Stream" /> object for the document, in read-only mode.
        /// </summary>
        /// <returns>
        /// The <see cref="Stream" /> object that can be read from.
        /// </returns>
        /// <exception cref="WebDAVSharp.Server.Exceptions.WebDavUnauthorizedException">If the user is unauthorized or has no access</exception>
        public Stream OpenReadStream()
        {
            Stream stream = null;
            try
            {
                // Impersonate the current user and create the file stream for opening the file
                WindowsImpersonationContext wic = Identity.Impersonate();
                stream = new FileStream(ItemPath, FileMode.Open, FileAccess.Read, FileShare.None);
                wic.Undo();
            }
            catch
            {
                throw new WebDavUnauthorizedException();
            }
            return stream;
        }

        /// <summary>
        /// Opens a <see cref="Stream" /> object for the document, in write-only mode.
        /// </summary>
        /// <param name="append">A value indicating whether to append to the existing document;
        /// if 
        /// <c>false</c>, the existing content will be dropped.</param>
        /// <param name="size"></param>
        /// <returns>
        /// The <see cref="Stream" /> object that can be written to.
        /// </returns>
        /// <exception cref="WebDAVSharp.Server.Exceptions.WebDavUnauthorizedException">If the user is unauthorized or has no access</exception>
        public Stream OpenWriteStream(bool append, long size)
        {
            if (append)
            {
                FileStream result = new FileStream(ItemPath, FileMode.Open, FileAccess.ReadWrite, FileShare.None);
                result.Seek(0, SeekOrigin.End);
                return result;
            }

            Stream stream = null;
            try
            {
                // Impersonate the current user and create the file stream for writing the file
                WindowsImpersonationContext wic = Identity.Impersonate();
                stream = new FileStream(ItemPath, FileMode.Create, FileAccess.ReadWrite, FileShare.None);
                wic.Undo();
            }
            catch
            {
                throw new WebDavUnauthorizedException();
            }
            return stream;
        }

        /// <summary>
        /// 
        /// </summary>
        public void FinishWriteOperation()
        {
            //Do nothing, I'm not interested.
        }

        #endregion
    }
}