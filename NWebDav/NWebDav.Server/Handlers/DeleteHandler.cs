using System;
using System.Threading.Tasks;
using System.Xml.Linq;

using NWebDav.Server.Helpers;
using NWebDav.Server.Http;
using NWebDav.Server.Stores;

namespace NWebDav.Server.Handlers
{
    /// <summary>
    /// Implementation of the DELETE method.
    /// </summary>
    /// <remarks>
    /// The specification of the WebDAV DELETE method can be found in the
    /// <see href="http://www.webdav.org/specs/rfc2518.html#METHOD_DELETE">
    /// WebDAV specification
    /// </see>.
    /// </remarks>
    public class DeleteHandler : IRequestHandler
    {
        /// <summary>
        /// Handle a DELETE request.
        /// </summary>
        /// <param name="httpContext">
        /// The HTTP context of the request.
        /// </param>
        /// <param name="store">
        /// Store that is used to access the collections and items.
        /// </param>
        /// <returns>
        /// A task that represents the asynchronous DELETE operation. The task
        /// will always return <see langword="true"/> upon completion.
        /// </returns>
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
                response.SetStatus(DavStatusCode.NotFound);
                return true;
            }

            // Obtain the item that actually is deleted
            var deleteItem = await parentCollection.GetItemAsync(splitUri.Name, httpContext).ConfigureAwait(false);
            if (deleteItem == null)
            {
                // Source not found
                response.SetStatus(DavStatusCode.NotFound);
                return true;
            }

            // Check if the item is locked
            if (deleteItem.LockingManager?.IsLocked(deleteItem) ?? false)
            {
                // Obtain the lock token
                var ifToken = request.GetIfLockToken();
                if (!deleteItem.LockingManager.HasLock(deleteItem, ifToken))
                {
                    response.SetStatus(DavStatusCode.Locked);
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
                response.SetStatus(status);
            }


            return true;
        }

        private async Task<DavStatusCode> DeleteItemAsync(IStoreCollection collection, string name, IHttpContext httpContext, WebDavUri baseUri, UriResultCollection errors)
        {
            // Obtain the actual item
            var deleteCollection = await collection.GetItemAsync(name, httpContext).ConfigureAwait(false) as IStoreCollection;
            if (deleteCollection != null)
            {
                // Determine the new base URI
                var subBaseUri = UriHelper.Combine(baseUri, name);

                // Delete all entries first
                foreach (var entry in await deleteCollection.GetItemsAsync(httpContext).ConfigureAwait(false))
                    await DeleteItemAsync(deleteCollection, entry.Name, httpContext, subBaseUri, errors).ConfigureAwait(false);
            }

            // Attempt to delete the item
            return await collection.DeleteItemAsync(name, httpContext).ConfigureAwait(false);
        }
    }
}
