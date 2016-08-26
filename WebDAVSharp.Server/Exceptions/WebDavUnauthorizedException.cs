using System;
using System.Net;

namespace WebDAVSharp.Server.Exceptions
{
    /// <summary>
    /// This exception is thrown when the user is not authorized to execute the request.
    /// Statuscode: 401 Unauthorized.
    /// </summary>
    [Serializable]
    public class WebDavUnauthorizedException : WebDavException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="WebDavNotFoundException" /> class.
        /// </summary>
        /// <param name="message">The exception message stating the reason for the exception being thrown.</param>
        /// <param name="innerException">The 
        /// <see cref="Exception" /> that is the cause for this exception;
        /// or 
        /// <c>null</c> if no inner exception is specified.</param>
        public WebDavUnauthorizedException(string message = null, Exception innerException = null)
            : base(HttpStatusCode.Unauthorized, message, innerException)
        {
            // Do nothing here
        }
    }
}