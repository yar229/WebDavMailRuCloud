using System;
using NWebDav.Server.Http;

namespace NWebDav.Server
{
    /// <summary>
    /// Factory responsible for returning the proper 
    /// <see cref="IRequestHandler">request handler</see> to handle the
    /// request.
    /// </summary>
    public interface IRequestHandlerFactory
    {
        /// <summary>
        /// Obtain the <see cref="IRequestHandler">request handler</see> that
        /// can process the specified request.
        /// </summary>
        /// <param name="httpContext">
        /// The HTTP context specifies the entire HTTP context for this
        /// request. In most cases only the <see cref="IHttpRequest.HttpMethod"/>
        /// of the request will specify which handler should be used.
        /// </param>
        /// <returns>
        /// The request handler that will further process the request.
        /// </returns>
        /// <remarks>
        /// <para>
        /// Each incoming HTTP request use a single request handler, so the
        /// factory is called for each HTTP request exactly one time (unless
        /// the request is not allowed due to authorization issues).
        /// </para>
        /// <para>
        /// Request handlers may either be created for each request or they can
        /// be reused. Multiple requests might be processed at the same time,
        /// so if a request handler is reused, then it must be thread-safe.
        /// </para>
        /// <para>
        /// If <see langword="null"/> is returned, then the status code
        /// <see cref="DavStatusCode.NotImplemented"/> is returned to the
        /// requester.
        /// </para>
        /// </remarks>
        /// <seealso cref="IRequestHandler"/>
        IRequestHandler GetRequestHandler(IHttpContext httpContext);
    }
}
