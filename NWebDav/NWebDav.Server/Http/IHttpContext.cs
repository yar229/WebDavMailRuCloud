using System.Threading.Tasks;

namespace NWebDav.Server.Http
{
    /// <summary>
    /// HTTP context interface.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The HTTP context specifies the context for the current WebDAV request.
    /// It's an abstraction of the underlying HTTP context implementation and
    /// it should contain the request, response and the session information.
    /// </para>
    /// <para>
    /// The HTTP context will typically be created after receiving the HTTP
    /// message from the HTTP listener. It is passed to the
    /// <see cref="IWebDavDispatcher">WebDAV dispatcher</see>, so it can be
    /// processed. The dispatcher passes it to the appropriate handler.
    /// Although the internal NWebDAV code will serialize access to the
    /// context (and its underlying request, response and session), it should
    /// be accessible from an arbitrary thread and not rely internally on the
    /// synchronization context (i.e. a call to a static property that returns
    /// the thread's current HTTP context might result in a <c>null</c> or
    /// invalid HTTP context.
    /// </para>
    /// </remarks>
    public interface IHttpContext
    {
        /// <summary>
        /// Gets the current HTTP request message.
        /// </summary>
        /// <value>HTTP request.</value>
        /// <remarks>
        /// Each HTTP context should have a valid request.
        /// </remarks>
        IHttpRequest Request { get; }

        /// <summary>
        /// Gets the current HTTP response message.
        /// </summary>
        /// <value>HTTP response.</value>
        /// <remarks>
        /// Each HTTP context should have a valid response.
        /// </remarks>
        IHttpResponse Response { get; }

        /// <summary>
        /// Gets the session belonging to the current request.
        /// </summary>
        /// <value>Session associated with this HTTP request.</value>
        /// <remarks>
        /// If sessions and/or authorization is not used, then it is allowed to
        /// set this property to <see langword="null"/>.
        /// </remarks>
        IHttpSession Session { get; }

        /// <summary>
        /// Close the context.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Each request will have its own HTTP context and the
        /// <seealso cref="IWebDavDispatcher"/> dispatching the request should
        /// make sure the context is closed at the end of the request. When
        /// this method completes the response should have been sent or it
        /// should be ready, so the underlying HTTP infrastructure can send
        /// it.
        /// </para>
        /// </remarks>
        Task CloseAsync();
    }
}