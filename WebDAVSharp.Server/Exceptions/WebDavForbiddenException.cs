using System;
using System.Net;

namespace WebDAVSharp.Server.Exceptions
{
    /// <summary>
    /// Statuscode: 403 Forbidden.
    /// </summary>
    public class WebDavForbiddenException : WebDavException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="WebDavForbiddenException" /> class.
        /// </summary>
        /// <param name="message">The exception message stating the reason for the exception being thrown.</param>
        /// <param name="innerException">The 
        /// <see cref="Exception" /> that is the cause for this exception;
        /// or 
        /// <c>null</c> if no inner exception is specified.</param>
        public WebDavForbiddenException(string message = null, Exception innerException = null)
            : base(HttpStatusCode.Forbidden, message, innerException)
        {
            // Do nothing here
        }
    }
}
