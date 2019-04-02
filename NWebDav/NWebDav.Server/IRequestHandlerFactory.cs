using NWebDav.Server.Http;

namespace NWebDav.Server
{
    /// <summary>
    /// Factory responsible for returning the proper
    /// <see cref="IRequestHandler"/> instance to handle the request.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This interface is used by the <see cref="WebDavDispatcher"/> to create
    /// the appropriate request handler. If the <see cref="WebDavDispatcher"/> 
    /// is replaced by a custom implementation, then this factory might never
    /// be used at all.
    /// </para>
    /// <para>
    /// The NWebDAV library provides a default implementation
    /// (<see cref="NWebDav.Server.RequestHandlerFactory"/>) that
    /// should be suitable for most situations. You can provide your own
    /// implementation if you wish to change the default mapping or replace
    /// a handler with your own.
    /// </para>
    /// </remarks>
    /// <seealso cref="NWebDav.Server.IRequestHandler"/>
    /// <seealso cref="NWebDav.Server.RequestHandlerFactory"/>
    public interface IRequestHandlerFactory
    {
        /// <summary>
        /// Obtain the <seealso cref="IRequestHandler">request handler</seealso>
        /// that can process the specified request.
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
        /// so if a request handler is reused, then it must be thread-safe and
        /// re-entrant.
        /// </para>
        /// <para>
        /// If <see langword="null"/> is returned, then the status code
        /// <see cref="DavStatusCode.NotImplemented"/> is returned to the
        /// requester.
        /// </para>
        /// </remarks>
        IRequestHandler GetRequestHandler(IHttpContext httpContext);
    }
}
