using System.Collections.Generic;
using NWebDav.Server;
using NWebDav.Server.Http;

namespace YaR.WebDavMailRu.CloudStore.Mailru
{
    public class RequestHandlerFactory : IRequestHandlerFactory 
    {
        private static readonly IDictionary<string, IRequestHandler> RequestHandlers = new Dictionary<string, IRequestHandler>
        {   
            { "COPY",      new NWebDav.Server.Handlers.CopyHandler() },
            { "DELETE",    new DeleteHandler() },
            { "GET",       new NWebDav.Server.Handlers.GetAndHeadHandler() },
            { "HEAD",      new NWebDav.Server.Handlers.GetAndHeadHandler() },
            { "LOCK",      new NWebDav.Server.Handlers.LockHandler() },
            { "MKCOL",     new NWebDav.Server.Handlers.MkcolHandler() },
            { "MOVE",      new MoveHandler() },
            { "OPTIONS",   new NWebDav.Server.Handlers.OptionsHandler() },
            { "PROPFIND",  new NWebDav.Server.Handlers.PropFindHandler() },
            { "PROPPATCH", new NWebDav.Server.Handlers.PropPatchHandler() },
            { "PUT",       new NWebDav.Server.Handlers.PutHandler() },
            { "UNLOCK",    new NWebDav.Server.Handlers.UnlockHandler() }
        };

        public IRequestHandler GetRequestHandler(IHttpContext httpContext)
        {
            if (!RequestHandlers.TryGetValue(httpContext.Request.HttpMethod, out var requestHandler))
                return null;

            return requestHandler;
        }

        public static IEnumerable<string> AllowedMethods => RequestHandlers.Keys;
    }
}
