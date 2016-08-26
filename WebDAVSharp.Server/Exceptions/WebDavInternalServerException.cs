using System;
using System.Net;

namespace WebDAVSharp.Server.Exceptions
{
    /// <summary>
    /// This exception is thrown when the server throws a different exception than the standard
    /// ones that 
    /// <see cref="WebDavServer" /> knows how to respond to.
    /// Statuscode: 500 Internal Server Error.
    /// </summary>
    [Serializable]
    public class WebDavInternalServerException : WebDavException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="WebDavInternalServerException" /> class.
        /// </summary>
        /// <param name="message">The exception message stating the reason for the exception being thrown.</param>
        /// <param name="innerException">The 
        /// <see cref="Exception" /> that is the cause for this exception;
        /// or 
        /// <c>null</c> if no inner exception is specified.</param>
        public WebDavInternalServerException(string message = null, Exception innerException = null)
            : base(HttpStatusCode.InternalServerError, message, innerException)
        {
            // Do nothing here
        }
    }
}