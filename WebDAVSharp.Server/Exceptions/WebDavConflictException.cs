using System;
using System.Net;

namespace WebDAVSharp.Server.Exceptions
{
    /// <summary>
    /// This exception is thrown when a request cannot be completed due to a conflict with the requested
    /// resource.
    /// Statuscode: 409 Conflict.
    /// </summary>
    [Serializable]
    public class WebDavConflictException : WebDavException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="WebDavConflictException" /> class.
        /// </summary>
        /// <param name="message">The exception message stating the reason for the exception being thrown.</param>
        /// <param name="innerException">The 
        /// <see cref="Exception" /> that is the cause for this exception;
        /// or 
        /// <c>null</c> if no inner exception is specified.</param>
        public WebDavConflictException(string message = null, Exception innerException = null)
            : base(HttpStatusCode.Conflict, message, innerException)
        {
            // Do nothing here
        }
    }
}