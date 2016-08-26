using System;
using System.Net;
using WebDAVSharp.Server.Adapters;

namespace WebDAVSharp.Server.Exceptions
{
    /// <summary>
    /// This exception is thrown when a request cannot be completed due to a conflict with the requested
    /// resource.
    /// Statuscode: 409 Conflict.
    /// </summary>
    [Serializable]
    public class WebDavLockedException : WebDavException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="WebDavLockedException" /> class.
        /// </summary>
        /// <param name="innerException">The 
        /// <see cref="Exception" /> that is the cause for this exception;
        /// or 
        /// <c>null</c> if no inner exception is specified.</param>
        /// <param name="errorMessage"></param>
        public WebDavLockedException(
            String errorMessage,
            Exception innerException = null)
            : base((HttpStatusCode) 423, errorMessage, innerException)
        {
            
        }

        /// <summary>
        /// https://www.ietf.org/proceedings/49/slides/WEBDAV-1/tsld008.htm
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public override string GetResponse(IHttpListenerContext context)
        {
            return String.Format(
@"<response-detail xmlns:""DAV:""> 
    <status-detail>
        <detail-code><child-was-locked /></detail-code>
        <user-text>{0}</user-text>
    </status-detail>
    <response>
        <href>{1}</href>
    <status>HTTP/1.1 423 Locked</status>
    <detail-code>
        <child-was-locked />
    </detail-code>
    </response>
</response-detail>", Message, context.Request.Url.AbsoluteUri);
        }

    }
}