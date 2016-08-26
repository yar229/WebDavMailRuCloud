using System.Collections.Generic;
using System.Xml;
using WebDAVSharp.Server.Adapters;
using WebDAVSharp.Server.Stores;
using WebDAVSharp.Server.Stores.Locks;

namespace WebDAVSharp.Server.MethodHandlers
{
    /// <summary>
    /// This class implements the <c>DELETE</c> HTTP method for WebDAV#.
    /// </summary>
    internal class WebDavDeleteMethodHandler : WebDavMethodHandlerBase
    {

        #region Properties

        /// <summary>
        /// Gets the collection of the names of the HTTP methods handled by this instance.
        /// </summary>
        /// <value>
        /// The names.
        /// </value>
        public override IEnumerable<string> Names
        {
            get
            {
                return new[]
                {
                    "DELETE"
                };
            }
        }

        #endregion

        #region Functions

        /// <summary>
        /// Processes the request.
        /// </summary>
        /// <param name="server">The <see cref="WebDavServer" /> through which the request came in from the client.</param>
        /// <param name="context">The
        /// <see cref="IHttpListenerContext" /> object containing both the request and response
        /// objects to use.</param>
        /// <param name="response"></param>
        /// <param name="request"></param>
        /// <param name="store">The <see cref="IWebDavStore" /> that the <see cref="WebDavServer" /> is hosting.</param>
        protected override void OnProcessRequest(
           WebDavServer server,
           IHttpListenerContext context,
           IWebDavStore store,
           XmlDocument request,
           XmlDocument response)
        {
            // Get the parent collection of the item
            IWebDavStoreCollection collection = GetParentCollection(server, store, context.Request.Url);

            // Get the item from the collection
            IWebDavStoreItem item = GetItemFromCollection(collection, context.Request.Url);

            // Deletes the item
            collection.Delete(item);
            //do not forget to clear all locks, if the object gets deleted there is no need to keep locks around.
            WebDavStoreItemLock.ClearLocks(context.Request.Url);

            context.SendSimpleResponse();
        }

        #endregion
    }
}
