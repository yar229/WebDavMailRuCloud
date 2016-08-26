using System.Collections.Generic;
using System.Xml;
using WebDAVSharp.Server.Adapters;
using WebDAVSharp.Server.Stores;

namespace WebDAVSharp.Server.MethodHandlers
{
    /// <summary>
    /// This interface must be implemented by a class that will respond
    /// to requests from a client by handling specific HTTP methods.
    /// </summary>
    public interface IWebDavMethodHandler
    {
        /// <summary>
        /// Gets the collection of the names of the HTTP methods handled by this instance.
        /// </summary>
        /// <value>
        /// The names.
        /// </value>
        IEnumerable<string> Names
        {
            get;
        }

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
        void ProcessRequest(
            WebDavServer server, 
            IHttpListenerContext context, 
            IWebDavStore store,
            out XmlDocument request,
            out XmlDocument response);
    }
}