using System;
using System.Net;

namespace WebDAVSharp.Server.Exceptions
{
    /// <summary>
    /// This exception is thrown when a request tries to access a resource that does not exist.
    /// Statuscode: 404 Not Found.
    /// </summary>
    [Serializable]
    public class WebDavNotFoundException : WebDavException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="WebDavNotFoundException" /> class.
        /// </summary>
        /// <param name="message">The exception message stating the reason for the exception being thrown.</param>
        /// <param name="innerException">The 
        /// <see cref="Exception" /> that is the cause for this exception;
        /// or 
        /// <c>null</c> if no inner exception is specified.</param>
        public WebDavNotFoundException(string message, Exception innerException = null)
            : base(HttpStatusCode.NotFound, message, innerException)
        {
            // Do nothing here
        }
    }
}