using System;

namespace NWebDav.Server.Http
{
    /// <summary>
    /// HTTP context interface.
    /// </summary>
    public interface IHttpContext
    {
        /// <summary>
        /// Gets the current HTTP request.
        /// </summary>
        /// <value>HTTP request.</value>
        IHttpRequest Request { get; }

        /// <summary>
        /// Gets the current HTTP response.
        /// </summary>
        /// <value>HTTP response.</value>
        IHttpResponse Response { get; }

        /// <summary>
        /// Gets the session belonging to the current request.
        /// </summary>
        /// <value>Session.</value>
        IHttpSession Session { get; }

        /// <summary>
        /// Close the context.
        /// </summary>
        /// <remarks>
        /// Contexts need to be closed to make sure the response is sent back
        /// to the requester. After the context is closed, it cannot be accessed
        /// anymore.
        /// </remarks>
        void Close();
    }
}