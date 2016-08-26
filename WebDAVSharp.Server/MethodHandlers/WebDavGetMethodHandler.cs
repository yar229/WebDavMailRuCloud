using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Web;
using System.Xml;
using WebDAVSharp.Server.Adapters;
using WebDAVSharp.Server.Exceptions;
using WebDAVSharp.Server.Stores;

namespace WebDAVSharp.Server.MethodHandlers
{
    /// <summary>
    /// This class implements the <c>GET</c> HTTP method for WebDAV#.
    /// </summary>
    internal sealed class WebDavGetMethodHandler : WebDavMethodHandlerBase
    {

        #region Properties

        /// <summary>
        /// Gets the collection of the names of the verbs handled by this instance.
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
                    "GET"
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
        /// <exception cref="WebDAVSharp.Server.Exceptions.WebDavNotFoundException"></exception>
        /// <exception cref="WebDavNotFoundException"><para>
        ///   <paramref name="context" /> specifies a request for a store item that does not exist.</para>
        /// <para>- or -</para>
        /// <para>
        ///   <paramref name="context" /> specifies a request for a store item that is not a document.</para></exception>
        /// <exception cref="WebDavConflictException"><paramref name="context" /> specifies a request for a store item using a collection path that does not exist.</exception>
        /// <param name="response"></param>
        /// <param name="request"></param>
       protected override void OnProcessRequest(
           WebDavServer server,
           IHttpListenerContext context,
           IWebDavStore store,
           XmlDocument request,
           XmlDocument response)
        { 
            IWebDavStoreCollection collection = GetParentCollection(server, store, context.Request.Url);
            IWebDavStoreItem item = GetItemFromCollection(collection, context.Request.Url);
            IWebDavStoreDocument doc = item as IWebDavStoreDocument;
            if (doc == null)
                throw new WebDavNotFoundException(string.Format("Cannot find document item  {0}", context.Request.Url));

            context.Response.SetEtag(doc.Etag);
            context.Response.SetLastModified(doc.ModificationDate);
            var extension = Path.GetExtension(doc.ItemPath);
            context.Response.AppendHeader("Content-Type", MimeMapping.GetMimeMapping(extension));

            context.Response.AppendHeader("Cache-Control", "no-cache");
            context.Response.AppendHeader("Pragma", "no-cache");
            context.Response.AppendHeader("Expires", "-1");
            context.Response.AppendHeader("Accept-Ranges", "bytes");
            context.Response.AppendHeader("Access-Control-Allow-Origin", "*");
            context.Response.AppendHeader("Access-Control-Allow-Credentials", "true");
            context.Response.AppendHeader("Access-Control-Allow-Methods", "ACL, CANCELUPLOAD, CHECKIN, CHECKOUT, COPY, DELETE, GET, HEAD, LOCK, MKCALENDAR, MKCOL, MOVE, OPTIONS, POST, PROPFIND, PROPPATCH, PUT, REPORT, SEARCH, UNCHECKOUT, UNLOCK, UPDATE, VERSION-CONTROL");
            context.Response.AppendHeader("Access-Control-Allow-Headers", "Overwrite, Destination, Content-Type, Depth, User-Agent, Translate, Range, Content-Range, Timeout, X-File-Size, X-Requested-With, If-Modified-Since, X-File-Name, Cache-Control, Location, Lock-Token, If");
            context.Response.AppendHeader("X-Engine", ".NETWebDav");
            context.Response.AppendHeader("MS-Author-Via", "DAV");
            context.Response.AppendHeader("Access-Control-Max-Age", "2147483647");
            context.Response.AppendHeader("Public", "");


            var ifModifiedSince = context.Request.Headers["If-Modified-Since"];
            var ifNoneMatch = context.Request.Headers["If-None-Match"];
            if (ifNoneMatch != null)
            {
                if (ifNoneMatch == doc.Etag)
                {
                    context.Response.StatusCode = 304;
                    context.Response.Close();
                    return;
                }
            }

            long docSize = doc.Size;
            if (docSize == 0)
            {
                context.Response.StatusCode = (int)HttpStatusCode.OK;
                context.Response.ContentLength64 = 0;
            }

            using (Stream stream = doc.OpenReadStream())
            {
                stream.Seek(0, SeekOrigin.Begin);

                if (stream == null)
                {
                    context.Response.StatusCode = (int)HttpStatusCode.OK;
                    context.Response.ContentLength64 = 0;
                }
                else
                {
                    context.Response.StatusCode = (int)HttpStatusCode.OK;

                    if (docSize > 0)
                        context.Response.ContentLength64 = docSize;

                    byte[] buffer = new byte[4096];
                    int inBuffer;
                    while ((inBuffer = stream.Read(buffer, 0, buffer.Length)) > 0)
                        context.Response.OutputStream.Write(buffer, 0, inBuffer);
                    context.Response.OutputStream.Flush();
                }
            }
            
            context.Response.Close();
        }

        #endregion
    }
}