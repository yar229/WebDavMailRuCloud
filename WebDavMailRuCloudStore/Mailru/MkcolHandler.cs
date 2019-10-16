using System.Threading.Tasks;
using NWebDav.Server;
using NWebDav.Server.Helpers;
using NWebDav.Server.Http;
using NWebDav.Server.Stores;

namespace YaR.WebDavMailRu.CloudStore.Mailru
{
    public class MkcolHandler : IRequestHandler
    {
        /// <summary>
        /// Handle a MKCOL request.
        /// </summary>
        /// <param name="httpContext">
        /// The HTTP context of the request.
        /// </param>
        /// <param name="store">
        /// Store that is used to access the collections and items.
        /// </param>
        /// <returns>
        /// A task that represents the asynchronous MKCOL operation. The task
        /// will always return <see langword="true"/> upon completion.
        /// </returns>
        public async Task<bool> HandleRequestAsync(IHttpContext httpContext, IStore store)
        {
            // Obtain request and response
            var request = httpContext.Request;
            var response = httpContext.Response;

            // The collection must always be created inside another collection
            var splitUri = request.Url.Parent;

            // Obtain the parent entry
            var collection = await store.GetCollectionAsync(splitUri.Parent, httpContext).ConfigureAwait(false);
            if (collection == null || !collection.IsValid) 
            {
                // Source not found
                response.SetStatus(DavStatusCode.Conflict);
                return true;
            }

            // Create the collection
            var result = await collection.CreateCollectionAsync(splitUri.Name, false, httpContext).ConfigureAwait(false);

            // Finished
            response.SetStatus(result.Result);
            return true;
        }
    }
}