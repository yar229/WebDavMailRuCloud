using System;
using System.Threading.Tasks;

using NWebDav.Server.Helpers;
using NWebDav.Server.Http;
using NWebDav.Server.Stores;

namespace NWebDav.Server.Handlers
{
    public class MkcolHandler : IRequestHandler
    {
        public async Task<bool> HandleRequestAsync(IHttpContext httpContext, IStore store)
        {
            // Obtain request and response
            var request = httpContext.Request;
            var response = httpContext.Response;

            // The collection must always be created inside another collection
            var splitUri = RequestHelper.SplitUri(request.Url);

            // Obtain the parent entry
            var collection = await store.GetCollectionAsync(splitUri.CollectionUri, httpContext).ConfigureAwait(false);
            if (collection == null)
            {
                // Source not found
                response.SendResponse(DavStatusCode.Conflict);
                return true;
            }

            // Create the collection
            var result = await collection.CreateCollectionAsync(splitUri.Name, false, httpContext).ConfigureAwait(false);

            // Finished
            response.SendResponse(result.Result);
            return true;
        }
    }
}
