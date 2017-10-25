using System.Threading.Tasks;

using NWebDav.Server.Helpers;
using NWebDav.Server.Http;
using NWebDav.Server.Stores;

namespace NWebDav.Server.Handlers
{
    /// <summary>
    /// Implementation of the OPTIONS method.
    /// </summary>
    /// <remarks>
    /// This implementation reports a class 1 and 2 compliant WebDAV server
    /// that supports all the standard WebDAV methods.
    /// </remarks>
    public class OptionsHandler : IRequestHandler
    {
        /// <summary>
        /// Handle a OPTIONS request.
        /// </summary>
        /// <param name="httpContext">
        /// The HTTP context of the request.
        /// </param>
        /// <param name="store">
        /// Store that is used to access the collections and items.
        /// </param>
        /// <returns>
        /// A task that represents the asynchronous OPTIONS operation. The task
        /// will always return <see langword="true"/> upon completion.
        /// </returns>
        public Task<bool> HandleRequestAsync(IHttpContext httpContext, IStore store)
        {
            // Obtain response
            var response = httpContext.Response;

            // We're a DAV class 1 and 2 compatible server
            response.SetHeaderValue("Dav", "1, 2");
            response.SetHeaderValue("MS-Author-Via", "DAV");

            // Set the Allow/Public headers
            response.SetHeaderValue("Allow", string.Join(", ", RequestHandlerFactory.AllowedMethods));
            response.SetHeaderValue("Public", string.Join(", ", RequestHandlerFactory.AllowedMethods));

            // Finished
            response.SetStatus(DavStatusCode.Ok);
            return Task.FromResult(true);
        }
    }
}
