using System;
using System.Diagnostics;
using System.IO;
using System.Security.Principal;
using System.Web;
using MailRuCloudApi;
using WebDAVSharp.Server.Exceptions;
using WebDAVSharp.Server.Stores;

using WebDAVSharp.Server.Utilities;
using File = MailRuCloudApi.File;

namespace WebDavMailRuCloudStore
{
    /// <summary>
    /// This class implements a disk-based <see cref="WebDavMailRuCloudStoreDocument" /> mapped to a file.
    /// </summary>
    [DebuggerDisplay("File ({Name})")]
    public sealed class WebDavMailRuCloudStoreDocument : WebDavMailRuCloudStoreItem, IWebDavStoreDocument
    {
        private readonly File _file;

        /// <summary>
        /// Initializes a new instance of the <see cref="WebDavMailRuCloudStoreDocument" /> class.
        /// </summary>
        /// <param name="parentCollection">The parent 
        /// <see cref="WebDavMailRuCloudStoreCollection" /> that contains this 
        /// <see cref="WebDavMailRuCloudStoreItem" />;
        /// or 
        /// <c>null</c> if this is the root 
        /// <see cref="WebDavMailRuCloudStoreCollection" />.</param>
        /// <param name="path">The path that this <see cref="WebDavMailRuCloudStoreItem" /> maps to.</param>
        /// <exception cref="ArgumentNullException"><paramref name="path" /> is <c>null</c> or empty.</exception>
        public WebDavMailRuCloudStoreDocument(WebDavMailRuCloudStoreCollection parentCollection, string path)
            : base(parentCollection, path)
        {
            // Do nothing here
        }

        public WebDavMailRuCloudStoreDocument(WebDavMailRuCloudStoreCollection parentCollection, File file) : this(parentCollection, file.FulPath)
        {
            _file = file;
        }

        #region IWebDAVStoreDocument Members

        /// <summary>
        /// Gets the size of the document in bytes.
        /// </summary>
        public long Size => _file?.Size?.DefaultValue ?? 0;

        /// <summary>
        /// Gets the mime type of this <see cref="IWebDavStoreItem" />.
        /// </summary>
        public string MimeType => MimeMapping.GetMimeMapping(Name);

        public override bool IsCollection => false;

        /// <summary>
        /// Gets the etag of this <see cref="IWebDavStoreItem" />.
        /// </summary>
        public string Etag => Md5Util.Md5HashStringForUtf8String(ItemPath + ModificationDate + Hidden + Size);

        #endregion

        #region IWebDAVStoreDocument Members

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
                WindowsImpersonationContext wic = Identity.Impersonate();
                stream = Cloud.GetFileStream(_file).Result;
                wic.Undo();
            }
            catch (Exception ex)
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
            var stream = Cloud.GetUploadStream(Name, ItemPath, ".bin", size);
            return stream;
        }

        public void FinishWriteOperation()
        {
            //throw new NotImplementedException();
        }


        private MailRuCloud Cloud => ((WebDavMailRuCloudStoreCollection)ParentCollection).Cloud;
        #endregion
    }
}
