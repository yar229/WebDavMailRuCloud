using System;
using System.Web;

namespace WebDAVSharp.Server.Stores.BaseClasses
{
    /// <summary>
    /// This is the base class for <see cref="IWebDavStoreItem" /> implementations.
    /// </summary>
    public class WebDavStoreDocumentBase : WebDavStoreItemBase
    {

        #region Constructor
        /// <summary>
        /// Initializes a new instance of the <see cref="WebDavStoreItemBase" /> class.
        /// </summary>
        /// <param name="parentCollection">The parent <see cref="IWebDavStoreCollection" /> that contains this <see cref="IWebDavStoreItem" /> implementation.</param>
        /// <param name="name">The name of this <see cref="IWebDavStoreItem" /></param>
        /// <exception cref="ArgumentNullException"><paramref name="name" /> is <c>null</c>.</exception>
        protected WebDavStoreDocumentBase(IWebDavStoreCollection parentCollection, string name) : base(parentCollection, name)
        {
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the mime type of this <see cref="IWebDavStoreItem" />.
        /// </summary>
        /// <value>
        /// The type of the MIME.
        /// </value>
        public string MimeType
        {
            get
            {
                return MimeMapping.GetMimeMapping(Name);
            }
        }

        #endregion

    }
}