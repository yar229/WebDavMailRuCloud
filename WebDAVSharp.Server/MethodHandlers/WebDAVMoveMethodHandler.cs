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
    /// This class implements the <c>MOVE</c> HTTP method for WebDAV#.
    /// </summary>
    internal class WebDavMoveMethodHandler : WebDavMethodHandlerBase
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
                    "MOVE"
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
        /// <param name="store">The <see cref="IWebDavStore" /> that the <see cref="WebDavServer" /> is hosting.</param>
        /// <param name="response"></param>
        /// <param name="request"></param>
        protected override void OnProcessRequest(
           WebDavServer server,
           IHttpListenerContext context,
           IWebDavStore store,
           XmlDocument request,
           XmlDocument response)
        {
            IWebDavStoreItem source = context.Request.Url.GetItem(server, store);

            MoveItem(server, context, store, source);
        }

        /// <summary>
        /// Moves the
        /// </summary>
        /// <param name="server">The <see cref="WebDavServer" /> through which the request came in from the client.</param>
        /// <param name="context">The 
        /// <see cref="IHttpListenerContext" /> object containing both the request and response
        /// objects to use.</param>
        /// <param name="store">The <see cref="IWebDavStore" /> that the <see cref="WebDavServer" /> is hosting.</param>
        /// <param name="sourceWebDavStoreItem">The <see cref="IWebDavStoreItem" /> that will be moved</param>
        /// <exception cref="WebDAVSharp.Server.Exceptions.WebDavForbiddenException">If the source path is the same as the destination path</exception>
        /// <exception cref="WebDAVSharp.Server.Exceptions.WebDavPreconditionFailedException">If one of the preconditions failed</exception>
        private static void MoveItem(WebDavServer server, IHttpListenerContext context, IWebDavStore store,
            IWebDavStoreItem sourceWebDavStoreItem)
        {
            Uri destinationUri = GetDestinationHeader(context.Request);
            IWebDavStoreCollection destinationParentCollection = GetParentCollection(server, store, destinationUri);

            bool isNew = true;

            string destinationName = destinationUri.GetLastSegment();
            IWebDavStoreItem destination = destinationParentCollection.GetItemByName(destinationName);
            if (destination != null)
            {
                if (sourceWebDavStoreItem.ItemPath == destination.ItemPath)
                    throw new WebDavForbiddenException();
                // if the overwrite header is F, statuscode = precondition failed 
                if (!GetOverwriteHeader(context.Request))
                    throw new WebDavPreconditionFailedException();
                // else delete destination and set isNew to false
                destinationParentCollection.Delete(destination);
                isNew = false;
            }

            destinationParentCollection.MoveItemHere(sourceWebDavStoreItem, destinationName);

            // send correct response
            context.SendSimpleResponse(isNew ? (int)HttpStatusCode.Created : (int)HttpStatusCode.NoContent);
        }

        #endregion
    }
}