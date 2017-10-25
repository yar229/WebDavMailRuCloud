using System;
using System.Collections.Generic;
using System.IO;

namespace NWebDav.Server.Http
{
    /// <summary>
    /// HTTP request message interface.
    /// </summary>
    /// <remarks>
    /// This interface contains all the HTTP request message related
    /// information for a request. It's important to distinguish the difference
    /// between the request and the session. The request is information that is
    /// unique per call, while session information is typically consistent
    /// during multiple calls within that session. The HTTP request message
    /// should be considered as a read-only object.
    /// </remarks>
    public interface IHttpRequest
    {
        /// <summary>
        /// Gets the HTTP method of the request.
        /// </summary>
        /// <value>The HTTP method.</value>
        /// <remarks>
        /// The <see cref="RequestHandlerFactory"/> uses this method to
        /// determine which handler should be called based on this property.
        /// A description of all the HTTP methods (verbs) can be found in 
        /// <see href="http://www.webdav.org/specs/rfc2518.html#http.methods.for.distributed.authoring">chapter 8 of the WebDAV specification</see>.
        /// </remarks>
        string HttpMethod { get; }

        /// <summary>
        /// Gets the URL of the request.
        /// </summary>
        /// <value>URL of the collection and/or document.</value>
        /// <remarks>
        /// The URL in a WebDAV request typically specifies which document or
        /// collection should be used for the request.
        /// </remarks>
        Uri Url { get; }

        /// <summary>
        /// Gets the remote end point of the request.
        /// </summary>
        /// <value>
        /// The remote endpoint of the originator of the request.
        /// </value>
        /// <remarks>
        /// <para>
        /// The remote endpoint is only used for logging by the internal
        /// NWebDAV code. Not all HTTP frameworks provide the remote endpoint
        /// and it might have been changed by (reverse) proxies, so don't rely
        /// on this information for any other use then logging.
        /// </para>
        /// <para>
        /// If you are implementing this interface for your own HTTP framework
        /// and you can't get the remote endpoint, then don't throw a
        /// <see cref="NotSupportedException"/> (or any other exception),
        /// because this will cause the request to fail.
        /// </para>
        /// </remarks>
        string RemoteEndPoint { get; }

        /// <summary>
        /// Gets all headers of the request.
        /// </summary>
        /// <value>All headers (key only) of this request.</value>
        /// <remarks>
        /// All HTTP headers keys should be returned as they appear in the
        /// request message. Although headers are case insensitive, they should
        /// be returned in the case as they appear in the request message
        /// (if supported by the underlying HTTP infrastructure).
        /// </remarks>
        IEnumerable<string> Headers { get; }

        /// <summary>
        /// Gets the value of a request header.
        /// </summary>
        /// <returns>The header value.</returns>
        /// <param name="header">
        /// Name of the header that should be obtained.
        /// </param>
        /// <remarks>
        /// <para>
        /// All header values should be returned as a string without any
        /// processing or rewriting.
        /// </para>
        /// <para>
        /// HTTP header keys are case insensitive, so fetching headers should
        /// no rely on casing.
        /// </para>
        /// </remarks>
        string GetHeaderValue(string header);

        /// <summary>
        /// Gets the HTTP request body stream.
        /// </summary>
        /// <value>HTTP request body stream.</value>
        /// <remarks>
        /// <para>
        /// The HTTP request body stream only needs to support forward-only
        /// reading of the stream. The internal NWebDAV code doesn't require
        /// to know the length of the stream and/or use positioning (seeking)
        /// within the stream.
        /// </para>
        /// </remarks>
        Stream Stream { get; }
    }
}