using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Xml;
using WebDAVSharp.Server.Adapters;
using WebDAVSharp.Server.Exceptions;
using WebDAVSharp.Server.Stores;

namespace WebDAVSharp.Server.MethodHandlers
{
    /// <summary>
    /// This class implements the <c>COPY</c> HTTP method for WebDAV#.
    /// </summary>
    internal class WebDavCopyMethodHandler : WebDavMethodHandlerBase
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
                    "COPY"
                };
            }
        }

        #endregion

        #region Public Functions

        /// <summary>
        /// Processes the request.
        /// </summary>
        /// <param name="server">The <see cref="WebDavServer" /> through which the request came in from the client.</param>
        /// <param name="context">The 
        /// <see cref="IHttpListenerContext" /> object containing both the request and response
        /// objects to use.</param>
        /// <param name="request"></param>
        /// <param name="response"></param>
        /// <param name="store">The <see cref="IWebDavStore" /> that the <see cref="WebDavServer" /> is hosting.</param>
        /// <exception cref="WebDAVSharp.Server.Exceptions.WebDavMethodNotAllowedException"></exception>
        protected override void OnProcessRequest(
                   WebDavServer server,
                   IHttpListenerContext context,
                   IWebDavStore store,
                   XmlDocument request,
                   XmlDocument response)
        {
            IWebDavStoreItem source = context.Request.Url.GetItem(server, store);
            if (source is IWebDavStoreDocument || source is IWebDavStoreCollection)
                CopyItem(server, context, store, source);
            else
                throw new WebDavMethodNotAllowedException();
        }

        /// <summary>
        /// Copies the item.
        /// </summary>
        /// <param name="server">The server.</param>
        /// <param name="context">The context.</param>
        /// <param name="store">The store.</param>
        /// <param name="source">The source.</param>
        /// <exception cref="WebDAVSharp.Server.Exceptions.WebDavForbiddenException"></exception>
        /// <exception cref="WebDAVSharp.Server.Exceptions.WebDavPreconditionFailedException"></exception>
        private static void CopyItem(WebDavServer server, IHttpListenerContext context, IWebDavStore store,
            IWebDavStoreItem source)
        {
            Uri destinationUri = GetDestinationHeader(context.Request);
            IWebDavStoreCollection destinationParentCollection = GetParentCollection(server, store, destinationUri);

            bool copyContent = (GetDepthHeader(context.Request) != 0);
            bool isNew = true;

            string destinationName = destinationUri.GetLastSegment();
            IWebDavStoreItem destination = destinationParentCollection.GetItemByName(destinationName);

            if (destination != null)
            {
                if (source.ItemPath == destination.ItemPath)
                    throw new WebDavForbiddenException();
                if (!GetOverwriteHeader(context.Request))
                    throw new WebDavPreconditionFailedException();
                if (destination is IWebDavStoreCollection)
                    destinationParentCollection.Delete(destination);
                isNew = false;
            }

            destinationParentCollection.CopyItemHere(source, destinationName, copyContent);

            context.SendSimpleResponse(isNew ? (int)HttpStatusCode.Created : (int)HttpStatusCode.NoContent);
        }

        #endregion

    }
}