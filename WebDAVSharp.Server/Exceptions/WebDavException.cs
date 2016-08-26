using System;
using System.Net;
using System.Runtime.Serialization;
using System.Security.Permissions;
using System.Web;
using WebDAVSharp.Server.Adapters;

namespace WebDAVSharp.Server.Exceptions
{
    /// <summary>
    /// This exception, or a descendant, is thrown when requests fail, specifying the status code
    /// that the server should return back to the client.
    /// </summary>
    [Serializable]
    public class WebDavException : HttpException
    {
        private const string StatusCodeKey = "StatusCode";
        private const string StatusDescriptionKey = "StatusDescription";

        /// <summary>
        /// Initializes a new instance of the <see cref="WebDavException" /> class.
        /// </summary>
        public WebDavException()
            : this(HttpStatusCode.InternalServerError)
        {
            // Do nothing here
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="WebDavException" /> class.
        /// </summary>
        /// <param name="message">The exception message stating the reason for the exception being thrown.</param>
        public WebDavException(string message)
            : this(HttpStatusCode.InternalServerError, message)
        {
            // Do nothing here
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="WebDavException" /> class.
        /// </summary>
        /// <param name="message">The exception message stating the reason for the exception being thrown.</param>
        /// <param name="innerException">The 
        /// <see cref="Exception" /> that is the cause for this exception;
        /// or 
        /// <c>null</c> if no inner exception is specified.</param>
        public WebDavException(string message, Exception innerException)
            : this(HttpStatusCode.InternalServerError, message, innerException)
        {
            // Do nothing here
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="WebDavException" /> class.
        /// </summary>
        /// <param name="info">The <see cref="T:System.Runtime.Serialization.SerializationInfo" /> that holds the serialized object data about the exception being thrown.</param>
        /// <param name="context">The <see cref="T:System.Runtime.Serialization.StreamingContext" /> that contains contextual information about the source or destination.</param>
        /// <exception cref="T:System.ArgumentNullException">The <paramref name="info" /> parameter is null.</exception>
        /// <exception cref="T:System.Runtime.Serialization.SerializationException">The class name is null or <see cref="P:System.Exception.HResult" /> is zero (0).</exception>
        protected WebDavException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            StatusCode = info.GetInt32(StatusCodeKey);
            StatusDescription = info.GetString(StatusDescriptionKey);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="WebDavException" /> class.
        /// </summary>
        /// <param name="statusCode">The HTTP status code that this <see cref="WebDavException" /> maps to.</param>
        /// <param name="message">The exception message stating the reason for the exception being thrown.</param>
        /// <param name="innerException">The 
        /// <see cref="Exception" /> that is the cause for this exception;
        /// or 
        /// <c>null</c> if no inner exception is specified.</param>
        public WebDavException(HttpStatusCode statusCode, string message = null, Exception innerException = null)
            : base(GetMessage(statusCode, message), innerException)
        {
            StatusCode = (int)statusCode;
            StatusDescription = HttpWorkerRequest.GetStatusDescription((int)statusCode);
        }

        /// <summary>
        /// Gets the HTTP status code that this <see cref="WebDavException" /> maps to.
        /// </summary>
        /// <value>
        /// The status code.
        /// </value>
        public int StatusCode
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the status description for the HTTP <see cref="StatusCode" />.
        /// </summary>
        /// <value>
        /// The status description.
        /// </value>
        public string StatusDescription
        {
            get;
            private set;
        }

        /// <summary>
        /// When overridden in a derived class, sets the <see cref="T:System.Runtime.Serialization.SerializationInfo" /> with information about the exception.
        /// </summary>
        /// <param name="info">The <see cref="T:System.Runtime.Serialization.SerializationInfo" /> that holds the serialized object data about the exception being thrown.</param>
        /// <param name="context">The <see cref="T:System.Runtime.Serialization.StreamingContext" /> that contains contextual information about the source or destination.</param>
        /// <exception cref="T:System.ArgumentNullException">The <paramref name="info" /> parameter is a null reference (Nothing in Visual Basic).</exception>
        [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue(StatusCodeKey, StatusCode);
            info.AddValue(StatusDescriptionKey, StatusDescription);
        }

        /// <summary>
        /// Gets the message.
        /// </summary>
        /// <param name="statusCode">The status code.</param>
        /// <param name="message">The message.</param>
        /// <returns>The message and the status description.</returns>
        private static string GetMessage(HttpStatusCode statusCode, string message)
        {
            string format = "%s";
            if (!String.IsNullOrWhiteSpace(message))
                format = message;

            return format.Replace("%s", HttpWorkerRequest.GetStatusDescription((int)statusCode));
        }

        /// <summary>
        /// Return the result that should be returned to the caller.
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public virtual string GetResponse(IHttpListenerContext context)
        {
            return this.Message;
        }
    }
}