using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace WebDAVSharp.Server.Exceptions
{
    /// <summary>
    /// Exception for bad requests
    /// </summary>
    [Serializable]
    public class WebDavBadRequestException : WebDavException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="WebDavBadRequestException" /> class.
        /// </summary>
        /// <param name="message">The exception message stating the reason for the exception being thrown.</param>
        /// <param name="innerException">The 
        /// <see cref="Exception" /> that is the cause for this exception;
        /// or 
        /// <c>null</c> if no inner exception is specified.</param>
        public WebDavBadRequestException(string message = null, Exception innerException = null)
            : base(HttpStatusCode.BadRequest, message, innerException)
        {
            // Do nothing here
        }
    }
}
