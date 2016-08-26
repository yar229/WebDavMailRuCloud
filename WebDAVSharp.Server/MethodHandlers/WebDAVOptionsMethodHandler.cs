using System.Collections.Generic;
using System.Xml;
using WebDAVSharp.Server.Adapters;
using WebDAVSharp.Server.Stores;
using System.Linq;
using WebDAVSharp.Server.Stores.Locks;
using System.Net;
using WebDAVSharp.Server.Exceptions;

namespace WebDAVSharp.Server.MethodHandlers
{
    /// <summary>
    /// This class implements the <c>OPTIONS</c> HTTP method for WebDAV#.
    /// </summary>
    internal class WebDavOptionsMethodHandler : WebDavMethodHandlerBase
    {
        #region Variables

        //Original list of allowed verbs 
        //private static readonly List<string> verbsAllowed = new List<string> { "OPTIONS", "TRACE", "GET", "HEAD", "POST", "COPY", "PROPFIND", "LOCK", "UNLOCK", "PUT", "DELETE", "MOVE", "MKCOL" };

        //private static readonly List<string> verbsAllowed = new List<string> { "GET", "POST", "OPTIONS", "HEAD", "MKCOL", "PUT", "PROPFIND", "PROPPATCH", "DELETE", "MOVE", "COPY", "GETLIB", "LOCK", "UNLOCK" };

        private static readonly List<string> verbsAllowed = new List<string> {
            "OPTIONS",
            "GET",
            "HEAD",
            "DELETE",
            "PROPFIND",
            "PUT",
            "PROPPATCH",
            "COPY",
            "DELETE",
            "MOVE",
            "MKCOL",
            "POST",
            "REPORT",
            "SEARCH" };

        //private static readonly List<string> verbsPublic = new List<string> { "OPTIONS", "GET", "HEAD", "PROPFIND", "PROPPATCH", "MKCOL", "PUT", "DELETE", "COPY", "MOVE", "LOCK", "UNLOCK" };

        #endregion

        #region Properties

        /// <summary>
        /// Gets the collection of the names of the HTTP methods handled by this instance.
        /// </summary>
        public override IEnumerable<string> Names
        {
            get
            {
                return new[]
                {
                    "OPTIONS"
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

            var baseVerbs = verbsAllowed.Aggregate((s1, s2) => s1 + ", " + s2);
            

            if (WebDavStoreItemLock.LockEnabled)
            {
                baseVerbs += ", LOCK, UNLOCK";
            }
            context.Response.AppendHeader("Content-Type", "text /html; charset=UTF-8");
            context.Response.AppendHeader("Allow", baseVerbs);
            context.Response.AppendHeader("Public", baseVerbs);

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

            //foreach (string verb in verbsAllowed)
            //    context.Response.AppendHeader("Allow", verb);

            //foreach (string verb in verbsPublic)
            //    context.Response.AppendHeader("Public", verb);

            // Sends 200 OK
            context.SendSimpleResponse();
        }

        #endregion
    }
}