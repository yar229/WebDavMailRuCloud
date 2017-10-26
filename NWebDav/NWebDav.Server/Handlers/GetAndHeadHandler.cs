using System;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;

using NWebDav.Server.Helpers;
using NWebDav.Server.Http;
using NWebDav.Server.Props;
using NWebDav.Server.Stores;

namespace NWebDav.Server.Handlers
{
    /// <summary>
    /// Implementation of the GET and HEAD method.
    /// </summary>
    /// <remarks>
    /// The specification of the WebDAV GET and HEAD methods for collections
    /// can be found in the
    /// <see href="http://www.webdav.org/specs/rfc2518.html#rfc.section.8.4">
    /// WebDAV specification
    /// </see>.
    /// </remarks>
    public class GetAndHeadHandler : IRequestHandler
    {
        /// <summary>
        /// Handle a GET or HEAD request.
        /// </summary>
        /// <param name="httpContext">
        /// The HTTP context of the request.
        /// </param>
        /// <param name="store">
        /// Store that is used to access the collections and items.
        /// </param>
        /// <returns>
        /// A task that represents the asynchronous GET or HEAD operation. The
        /// task will always return <see langword="true"/> upon completion.
        /// </returns>
        public async Task<bool> HandleRequestAsync(IHttpContext httpContext, IStore store)
        {
            // Obtain request and response
            var request = httpContext.Request;
            var response = httpContext.Response;

            // Determine if we are invoked as HEAD
            var head = request.HttpMethod == "HEAD";

            // Determine the requested range
            var range = request.GetRange();

            // Obtain the WebDAV collection
            var entry = await store.GetItemAsync(request.Url, httpContext).ConfigureAwait(false);
            if (entry == null)
            {
                // Set status to not found
                response.SetStatus(DavStatusCode.NotFound);
                return true;
            }

            // Add non-expensive headers based on properties
            var propertyManager = entry.PropertyManager;
            if (propertyManager != null)
            {
                // Add Last-Modified header
                var lastModifiedUtc = (string)(await propertyManager.GetPropertyAsync(httpContext, entry, DavGetLastModified<IStoreItem>.PropertyName, true).ConfigureAwait(false));
                if (lastModifiedUtc != null)
                    response.SetHeaderValue("Last-Modified", lastModifiedUtc);

                // Add ETag
                var etag = (string)(await propertyManager.GetPropertyAsync(httpContext, entry, DavGetEtag<IStoreItem>.PropertyName, true).ConfigureAwait(false));
                if (etag != null)
                    response.SetHeaderValue("Etag", etag);

                // Add type
                var contentType = (string)(await propertyManager.GetPropertyAsync(httpContext, entry, DavGetContentType<IStoreItem>.PropertyName, true).ConfigureAwait(false));
                if (contentType != null)
                    response.SetHeaderValue("Content-Type", contentType);

                // Add language
                var contentLanguage = (string)(await propertyManager.GetPropertyAsync(httpContext, entry, DavGetContentLanguage<IStoreItem>.PropertyName, true).ConfigureAwait(false));
                if (contentLanguage != null)
                    response.SetHeaderValue("Content-Language", contentLanguage);
            }

            // Stream the actual entry
            using (var stream = await entry.GetReadableStreamAsync(httpContext).ConfigureAwait(false))
            {
                if (stream != null && stream != Stream.Null)
                {
                    // Set the response
                    response.SetStatus(DavStatusCode.Ok);

                    // Set the expected content length
                    try
                    {
                        // We can only specify the Content-Length header if the
                        // length is known (this is typically true for seekable streams)
                        if (stream.CanSeek)
                        {
                            // Add a header that we accept ranges (bytes only)
                            response.SetHeaderValue("Accept-Ranges", "bytes");

                            // Determine the total length
                            var length = stream.Length;

                            // Check if an 'If-Range' was specified
                            if (range?.If != null)
                            {
                                var lastModifiedText = (string)await propertyManager.GetPropertyAsync(httpContext, entry, DavGetLastModified<IStoreItem>.PropertyName, true).ConfigureAwait(false);
                                var lastModified = DateTime.Parse(lastModifiedText, CultureInfo.InvariantCulture);
                                if (lastModified != range.If)
                                    range = null;
                            }

                            // Check if a range was specified
                            if (range != null)
                            {
                                var start = range.Start ?? 0;
                                var end = Math.Min(range.End ?? long.MaxValue, length-1);
                                length = end - start + 1;

                                // Write the range
                                response.SetHeaderValue("Content-Range", $"bytes {start}-{end} / {stream.Length}");

                                // Set status to partial result if not all data can be sent
                                if (length < stream.Length)
                                    response.SetStatus(DavStatusCode.PartialContent);
                            }

                            // Set the header, so the client knows how much data is required
                            response.SetHeaderValue("Content-Length", $"{length}");
                        }
                    }
                    catch (NotSupportedException)
                    {
                        // If the content length is not supported, then we just skip it
                    }

                    // HEAD method doesn't require the actual item data
                    if (!head)
                        await CopyToAsync(stream, response.Stream, range?.Start ?? 0, range?.End).ConfigureAwait(false);
                }
                else
                {
                    // Set the response
                    response.SetStatus(DavStatusCode.NoContent);
                }
            }
            return true;
        }

        private async Task CopyToAsync(Stream src, Stream dest, long start, long? end)
        {
            // Skip to the first offset
            if (start > 0)
            {
                // We prefer seeking instead of draining data
                if (!src.CanSeek)
                    throw new IOException("Cannot use range, because the source stream isn't seekable");
                
                src.Seek(start, SeekOrigin.Begin);
            }

            // Determine the number of bytes to read
            var bytesToRead = end.HasValue ? end.Value - start + 1 : long.MaxValue;

            // Read in 64KB blocks
            var buffer = new byte[64 * 1024];

            // Copy, until we don't get any data anymore
            while (bytesToRead > 0)
            {
                // Read the requested bytes into memory
                var requestedBytes = (int)Math.Min(bytesToRead, buffer.Length);
                var bytesRead = await src.ReadAsync(buffer, 0, requestedBytes).ConfigureAwait(false);

                // We're done, if we cannot read any data anymore
                if (bytesRead == 0)
                    return;
                
                // Write the data to the destination stream
                await dest.WriteAsync(buffer, 0, bytesRead).ConfigureAwait(false);

                // Decrement the number of bytes left to read
                bytesToRead -= bytesRead;
            }
        }
    }
}
