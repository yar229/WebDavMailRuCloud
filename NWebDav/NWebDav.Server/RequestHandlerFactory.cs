using System.Collections.Generic;
using NWebDav.Server.Handlers;
using NWebDav.Server.Http;

namespace NWebDav.Server
{
    /// <summary>
    /// Default implementation of the <see cref="IRequestHandlerFactory"/>
    /// interface to create WebDAV request handlers. 
    /// </summary>
    /// <seealso cref="IRequestHandler"/>
    /// <seealso cref="IRequestHandlerFactory"/>
    public class RequestHandlerFactory : IRequestHandlerFactory
    {
        private static readonly IDictionary<string, IRequestHandler> s_requestHandlers = new Dictionary<string, IRequestHandler>
        {
            { "COPY", new CopyHandler() },
            { "DELETE", new DeleteHandler() },
            { "GET", new GetAndHeadHandler() },
            { "HEAD", new GetAndHeadHandler() },
            { "LOCK", new LockHandler() },
            { "MKCOL", new MkcolHandler() },
            { "MOVE", new MoveHandler() },
            { "OPTIONS", new OptionsHandler() },
            { "PROPFIND", new PropFindHandler() },
            { "PROPPATCH", new PropPatchHandler() },
            { "PUT", new PutHandler() },
            { "UNLOCK", new UnlockHandler() }
        };

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
        /// This implementation creates a new instance of the appropriate
        /// request handler for each request.
        /// </remarks>
        public IRequestHandler GetRequestHandler(IHttpContext httpContext)
        {
            // Obtain the dispatcher
            if (!s_requestHandlers.TryGetValue(httpContext.Request.HttpMethod, out var requestHandler))
                return null;

            // Create an instance of the request handler
            return requestHandler;
        }

        /// <summary>
        /// Gets a list of supported HTTP methods.
        /// </summary>
        public static IEnumerable<string> AllowedMethods => s_requestHandlers.Keys;
    }
}
