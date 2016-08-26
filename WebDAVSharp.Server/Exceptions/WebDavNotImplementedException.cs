using System;
using System.Net;

namespace WebDAVSharp.Server.Exceptions
{
    /// <summary>
    /// This exception is thrown when a request uses a HTTP method or functionality that has yet to
    /// be implemented.
    /// Statuscode: 501 Not Implemented.
    /// </summary>
    [Serializable]
    public class WebDavNotImplementedException : WebDavException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="WebDavNotImplementedException" /> class.
        /// </summary>
        /// <param name="message">The exception message stating the reason for the exception being thrown.</param>
        /// <param name="innerException">The 
        /// <see cref="Exception" /> that is the cause for this exception;
        /// or 
        /// <c>null</c> if no inner exception is specified.</param>
        public WebDavNotImplementedException(string message = null, Exception innerException = null)
            : base(HttpStatusCode.NotImplemented, message, innerException)
        {
            // Do nothing here
        }
    }
}