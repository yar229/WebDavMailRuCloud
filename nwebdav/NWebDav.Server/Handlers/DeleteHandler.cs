using System;
using System.Threading.Tasks;
using System.Xml.Linq;

using NWebDav.Server.Helpers;
using NWebDav.Server.Http;
using NWebDav.Server.Stores;

namespace NWebDav.Server.Handlers
{
    public class DeleteHandler : IRequestHandler
    {
        public async Task<bool> HandleRequestAsync(IHttpContext httpContext, IStore store)
        {
            // Obtain request and response
            var request = httpContext.Request;
            var response = httpContext.Response;

            // Keep track of all errors
            var errors = new UriResultCollection();

            // We should always remove the item from a parent container
            var splitUri = RequestHelper.SplitUri(request.Url);

            // Obtain parent collection
            var parentCollection = await store.GetCollectionAsync(splitUri.CollectionUri, httpContext).ConfigureAwait(false);
            if (parentCollection == null)
            {
                // Source not found
                response.SendResponse(DavStatusCode.NotFound);
                return true;
            }

            // Obtain the item that actually is deleted
            var deleteItem = await parentCollection.GetItemAsync(splitUri.Name, httpContext).ConfigureAwait(false);
            if (deleteItem == null)
            {
                // Source not found
                response.SendResponse(DavStatusCode.NotFound);
                return true;
            }

            // Check if the item is locked
            if (deleteItem.LockingManager?.IsLocked(deleteItem) ?? false)
            {
                // Obtain the lock token
                var ifToken = request.GetIfLockToken();
                if (!deleteItem.LockingManager.HasLock(deleteItem, ifToken))
                {
                    response.SendResponse(DavStatusCode.Locked);
                    return true;
                }

                // Remove the token
                deleteItem.LockingManager.Unlock(deleteItem, ifToken);
            }

            // Delete item
            var status = await DeleteItemAsync(parentCollection, splitUri.Name, httpContext, splitUri.CollectionUri, errors).ConfigureAwait(false);
            if (status == DavStatusCode.Ok && errors.HasItems)
            {
                // Obtain the status document
                var xDocument = new XDocument(errors.GetXmlMultiStatus());

                // Stream the document
                await response.SendResponseAsync(DavStatusCode.MultiStatus, xDocument).ConfigureAwait(false);
            }
            else
            {
                // Return the proper status
                response.SendResponse(status);
            }


            return true;
        }

        private async Task<DavStatusCode> DeleteItemAsync(IStoreCollection collection, string name, IHttpContext httpContext, Uri baseUri, UriResultCollection errors)
        {
            // Obtain the actual item
            var deleteCollection = await collection.GetItemAsync(name, httpContext).ConfigureAwait(false) as IStoreCollection;
            if (deleteCollection != null)
            {
                // Determine the new base URI
                var subBaseUri = UriHelper.Combine(baseUri, name);

                // Delete all entries first
                foreach (var entry in await deleteCollection.GetItemsAsync(httpContext).ConfigureAwait(false))
                    await DeleteItemAsync(deleteCollection, entry.Name, httpContext, subBaseUri, errors);
            }

            // Attempt to delete the item
            return await collection.DeleteItemAsync(name, httpContext).ConfigureAwait(false);
        }
    }
}
