using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Xml;
using WebDAVSharp.Server.Adapters;
using WebDAVSharp.Server.Exceptions;
using WebDAVSharp.Server.Stores;
using WebDAVSharp.Server.Stores.Locks;

namespace WebDAVSharp.Server.MethodHandlers
{
    /// <summary>
    /// This class implements the <c>PUT</c> HTTP method for WebDAV#.
    /// </summary>
    internal class WebDavUnlockMethodHandler : WebDavMethodHandlerBase
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
                    "UNLOCK"
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
            if (!WebDavStoreItemLock.LockEnabled) throw new WebDavNotImplementedException("Lock support disabled");

            /***************************************************************************************************
            * Send the response
            ***************************************************************************************************/
            WindowsIdentity Identity = (WindowsIdentity)Thread.GetData(Thread.GetNamedDataSlot(WebDavServer.HttpUser));
            var unlockResult = WebDavStoreItemLock.UnLock(context.Request.Url, GetLockTokenHeader(context.Request), Identity.Name);

            IWebDavStoreCollection collection = GetParentCollection(server, store, context.Request.Url);
            try
            {
                var item = GetItemFromCollection(collection, context.Request.Url);
                if (item != null)
                {
                    //we already have an item
                    var resourceCanBeUnLocked = item.UnLock(Identity.Name);
                    if (!resourceCanBeUnLocked)
                    {
                        //TODO: decide what to do if the resource cannot be locked.
                    }
                }
            }
            catch (Exception ex)
            {
               WebDavServer.Log.Warn(
                   String.Format("Request unlock on a resource that does not exists: {0}", context.Request.Url), ex);
            }

            context.SendSimpleResponse(unlockResult);
        }

        #endregion

    }
}