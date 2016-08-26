using System;
using System.Net;

namespace WebDAVSharp.Server.Exceptions
{
    /// <summary>
    /// This exception is thrown when the media type is unsupported.
    /// Statuscode: 415 Unsupported Media Type
    /// </summary>
    [Serializable]
    public class WebDavUnsupportedMediaTypeException : WebDavException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="WebDavUnsupportedMediaTypeException" /> class.
        /// </summary>
        /// <param name="message">The exception message stating the reason for the exception being thrown.</param>
        /// <param name="innerException">The 
        /// <see cref="Exception" /> that is the cause for this exception;
        /// or 
        /// <c>null</c> if no inner exception is specified.</param>
        public WebDavUnsupportedMediaTypeException(string message = null, Exception innerException = null)
            : base(HttpStatusCode.UnsupportedMediaType, message, innerException)
        {
            // Do nothing here
        }
    }
}