using System;
using System.Threading.Tasks;
using System.Xml.Linq;

using NWebDav.Server.Helpers;
using NWebDav.Server.Http;
using NWebDav.Server.Stores;

namespace NWebDav.Server.Handlers
{
    public class MoveHandler : IRequestHandler
    {
        public async Task<bool> HandleRequestAsync(IHttpContext httpContext, IStore store)
        {
            // Obtain request and response
            var request = httpContext.Request;
            var response = httpContext.Response;
            
            // We should always move the item from a parent container
            var splitSourceUri = RequestHelper.SplitUri(request.Url);

            // Obtain source collection
            var sourceCollection = await store.GetCollectionAsync(splitSourceUri.CollectionUri, httpContext).ConfigureAwait(false);
            if (sourceCollection == null)
            {
                // Source not found
                response.SendResponse(DavStatusCode.NotFound);
                return true;
            }

            // Obtain the destination
            var destinationUri = request.GetDestinationUri();
            if (destinationUri == null)
            {
                // Bad request
                response.SendResponse(DavStatusCode.BadRequest, "Destination header is missing.");
                return true;
            }

            // Make sure the source and destination are different
            if (request.Url.AbsoluteUri.Equals(destinationUri.AbsoluteUri, StringComparison.CurrentCultureIgnoreCase))
            {
                // Forbidden
                response.SendResponse(DavStatusCode.Forbidden, "Source and destination cannot be the same.");
                return true;
            }

            // We should always move the item to a parent
            var splitDestinationUri = RequestHelper.SplitUri(destinationUri);

            // Obtain destination collection
            var destinationCollection = await store.GetCollectionAsync(splitDestinationUri.CollectionUri, httpContext).ConfigureAwait(false);
            if (destinationCollection == null)
            {
                // Source not found
                response.SendResponse(DavStatusCode.NotFound);
                return true;
            }

            // Check if the Overwrite header is set
            var overwrite = request.GetOverwrite();

            // Keep track of all errors
            var errors = new UriResultCollection();

            // Move collection
            await MoveAsync(sourceCollection, splitSourceUri.Name, destinationCollection, splitDestinationUri.Name, overwrite, httpContext, splitDestinationUri.CollectionUri, errors).ConfigureAwait(false);

            // Check if there are any errors
            if (errors.HasItems)
            {
                // Obtain the status document
                var xDocument = new XDocument(errors.GetXmlMultiStatus());

                // Stream the document
                await response.SendResponseAsync(DavStatusCode.MultiStatus, xDocument).ConfigureAwait(false);
            }
            else
            {
                // Set the response
                response.SendResponse(DavStatusCode.Ok);
            }

            return true;
        }

        private async Task MoveAsync(IStoreCollection sourceCollection, string sourceName, IStoreCollection destinationCollection, string destinationName, bool overwrite, IHttpContext httpContext, Uri baseUri, UriResultCollection errors)
        {
            // Determine the new base URI
            var subBaseUri = UriHelper.Combine(baseUri, destinationName);

            // Obtain the actual item
            var moveCollection = await sourceCollection.GetItemAsync(sourceName, httpContext).ConfigureAwait(false) as IStoreCollection;
            if (moveCollection != null)
            {
                // Create a new collection
                var newCollectionResult = await destinationCollection.CreateCollectionAsync(destinationName, overwrite, httpContext);
                if (newCollectionResult.Result != DavStatusCode.Created && newCollectionResult.Result != DavStatusCode.NoContent)
                {
                    errors.AddResult(subBaseUri, newCollectionResult.Result);
                    return;
                }

                // Move all subitems
                foreach (var entry in await moveCollection.GetItemsAsync(httpContext).ConfigureAwait(false))
                    await MoveAsync(moveCollection, entry.Name, newCollectionResult.Collection, entry.Name, overwrite, httpContext, subBaseUri, errors);

                // Delete the source collection
                var deleteResult = await sourceCollection.DeleteItemAsync(sourceName, httpContext);
                if (deleteResult != DavStatusCode.Ok)
                {
                    errors.AddResult(subBaseUri, newCollectionResult.Result);
                    return;
                }
            }
            else
            {
                // Items should be moved directly
                var result = await sourceCollection.MoveItemAsync(sourceName, destinationCollection, destinationName, overwrite, httpContext);
                if (result.Result != DavStatusCode.Created && result.Result != DavStatusCode.NoContent)
                {
                    errors.AddResult(subBaseUri, result.Result);
                    return;
                }
            }
        }
    }
}
