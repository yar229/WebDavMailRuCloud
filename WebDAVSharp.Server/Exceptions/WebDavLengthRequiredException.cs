using System;
using System.Net;

namespace WebDAVSharp.Server.Exceptions
{
    /// <summary>
    /// Statuscode: 411 Length Required.
    /// </summary>
    [Serializable]
    public class WebDavLengthRequiredException : WebDavException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="WebDavLengthRequiredException" /> class.
        /// </summary>
        /// <param name="message">The exception message stating the reason for the exception being thrown.</param>
        /// <param name="innerException">The 
        /// <see cref="Exception" /> that is the cause for this exception;
        /// or 
        /// <c>null</c> if no inner exception is specified.</param>
        public WebDavLengthRequiredException(string message = null, Exception innerException = null)
            : base(HttpStatusCode.LengthRequired, message, innerException)
        {
            // Do nothing here
        }
    }
}