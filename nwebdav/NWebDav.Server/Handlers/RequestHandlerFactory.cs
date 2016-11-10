using System;
using System.Collections.Generic;

using NWebDav.Server.Http;

namespace NWebDav.Server.Handlers
{
    public class RequestHandlerFactory : IRequestHandlerFactory
    {
        private static readonly IDictionary<string, IRequestHandler> s_requestHandlers = new Dictionary<string, IRequestHandler>
        {
            // Yes, we could have done this using .NET attribute :-)
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

        public IRequestHandler GetRequestHandler(IHttpContext httpContext)
        {
            // Obtain the dispatcher
            IRequestHandler requestHandler;
            if (!s_requestHandlers.TryGetValue(httpContext.Request.HttpMethod, out requestHandler))
                return null;

            // Create an instance of the request handler
            return requestHandler;
        }

        public static IEnumerable<string> AllowedMethods => s_requestHandlers.Keys;

    }
}
