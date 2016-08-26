using System;
using System.Net;

namespace WebDAVSharp.Server.Exceptions
{
    /// <summary>
    /// This exception is thrown when one of the preconditions failed.
    /// Statuscode: 412 Precondition Failed.
    /// </summary>
    public class WebDavPreconditionFailedException : WebDavException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="WebDavPreconditionFailedException" /> class.
        /// </summary>
        /// <param name="message">The exception message stating the reason for the exception being thrown.</param>
        /// <param name="innerException">The 
        /// <see cref="Exception" /> that is the cause for this exception;
        /// or 
        /// <c>null</c> if no inner exception is specified.</param>
        public WebDavPreconditionFailedException(string message = null, Exception innerException = null)
            : base(HttpStatusCode.PreconditionFailed, message, innerException)
        {
            // Do nothing here
        }
    }
}
