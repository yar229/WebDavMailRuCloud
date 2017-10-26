using System.Threading.Tasks;

using NWebDav.Server.Http;

namespace NWebDav.Server
{
    /// <summary>
    /// Interface that is responsible for dispatching WebDAV requests.
    /// </summary>
    /// <remarks>
    /// The WebDAV dispatcher handles the processing of a WebDAV request. The
    /// library provides a default implementations (<see cref="WebDavDispatcher"/>)
    /// that dispatches WebDAV request based on the <see cref="IRequestHandler"/> and
    /// <see cref="IRequestHandlerFactory"/> interfaces. Although this implementation
    /// should suffice for most situations, it is possible to completely replace the
    /// request handling by using your own implementation.
    /// </remarks>
    /// <seealso cref="WebDavDispatcher"/>
    public interface IWebDavDispatcher
    {
        /// <summary>
        /// Dispatch the WebDAV request based on the given HTTP context.
        /// </summary>
        /// <param name="httpContext">
        /// HTTP context for this request.
        /// </param>
        /// <returns>
        /// A task that represents the request dispatching operation.
        /// </returns>
        Task DispatchRequestAsync(IHttpContext httpContext);
    }
}