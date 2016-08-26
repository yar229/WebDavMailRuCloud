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
    /// This class implements the <c>MKCOL</c> HTTP method for WebDAV#.
    /// </summary>
    internal class WebDavMkColMethodHandler : WebDavMethodHandlerBase
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
                    "MKCOL"
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
        /// <exception cref="WebDAVSharp.Server.Exceptions.WebDavUnsupportedMediaTypeException"></exception>
        /// <exception cref="WebDAVSharp.Server.Exceptions.WebDavMethodNotAllowedException"></exception>
        /// <param name="response"></param>
        /// <param name="request"></param>
         protected override void OnProcessRequest(
           WebDavServer server,
           IHttpListenerContext context,
           IWebDavStore store,
           XmlDocument request,
           XmlDocument response)
        {
            if (context.Request.ContentLength64 > 0)
                throw new WebDavUnsupportedMediaTypeException();

            IWebDavStoreCollection collection = GetParentCollection(server, store, context.Request.Url);
                
            string collectionName = context.Request.Url.GetLastSegment();
            IWebDavStoreItem item;
            if ((item = collection.GetItemByName(collectionName)) != null)
            {
                WebDavServer.Log.WarnFormat("MKCOL Failed: item {0} already exists as child of {1}. ",
                    collectionName, collection.ItemPath);
                throw new WebDavMethodNotAllowedException();
            }
                
              
            collection.CreateCollection(collectionName);

            context.SendSimpleResponse((int)HttpStatusCode.Created);
        }

        #endregion
    }
}