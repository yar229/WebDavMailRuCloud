using System.Threading.Tasks;

using NWebDav.Server.Helpers;
using NWebDav.Server.Http;
using NWebDav.Server.Stores;

namespace NWebDav.Server.Handlers
{
    /// <summary>
    /// Implementation of the UNLOCK method.
    /// </summary>
    /// <remarks>
    /// The specification of the WebDAV UNLOCK method can be found in the
    /// <see href="http://www.webdav.org/specs/rfc2518.html#METHOD_UNLOCK">
    /// WebDAV specification
    /// </see>.
    /// </remarks>
    public class UnlockHandler : IRequestHandler
    {
        /// <summary>
        /// Handle a UNLOCK request.
        /// </summary>
        /// <param name="httpContext">
        /// The HTTP context of the request.
        /// </param>
        /// <param name="store">
        /// Store that is used to access the collections and items.
        /// </param>
        /// <returns>
        /// A task that represents the asynchronous UNLOCK operation. The task
        /// will always return <see langword="true"/> upon completion.
        /// </returns>
        public async Task<bool> HandleRequestAsync(IHttpContext httpContext, IStore store)
        {
            // Obtain request and response
            var request = httpContext.Request;
            var response = httpContext.Response;

            // Obtain the lock-token
            var lockToken = request.GetLockToken();

            // Obtain the WebDAV item
            var item = await store.GetItemAsync(request.Url, httpContext).ConfigureAwait(false);
            if (item == null)
            {
                // Set status to not found
                response.SetStatus(DavStatusCode.PreconditionFailed);
                return true;
            }

            // Check if we have a lock manager
            var lockingManager = item.LockingManager;
            if (lockingManager == null)
            {
                // Set status to not found
                response.SetStatus(DavStatusCode.PreconditionFailed);
                return true;
            }

            // Perform the lock
            var result = lockingManager.Unlock(item, lockToken);

            // Send response
            response.SetStatus(result);
            return true;
        }
    }
}
