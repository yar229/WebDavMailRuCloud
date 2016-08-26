using System;
using System.Net;

namespace WebDAVSharp.Server.Exceptions
{
    /// <summary>
    /// This exception is thrown when a request uses a HTTP method to request or manipulate a resource
    /// for which the specified HTTP method is not allowed.
    /// Statuscode: 405 Method Not Allowed.
    /// </summary>
    [Serializable]
    public class WebDavMethodNotAllowedException : WebDavException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="WebDavMethodNotAllowedException" /> class.
        /// </summary>
        /// <param name="message">The exception message stating the reason for the exception being thrown.</param>
        /// <param name="innerException">The 
        /// <see cref="Exception" /> that is the cause for this exception;
        /// or 
        /// <c>null</c> if no inner exception is specified.</param>
        public WebDavMethodNotAllowedException(string message = null, Exception innerException = null)
            : base(HttpStatusCode.MethodNotAllowed, message, innerException)
        {
            // Do nothing here
        }
    }
}