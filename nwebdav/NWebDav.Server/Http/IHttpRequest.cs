using System;
using System.Collections.Generic;
using System.IO;
using System.Net;

namespace NWebDav.Server.Http
{
    /// <summary>
    /// HTTP request interface.
    /// </summary>
    public interface IHttpRequest
    {
        /// <summary>
        /// Gets the HTTP method of the request.
        /// </summary>
        /// <value>The HTTP method.</value>
        string HttpMethod { get; }

        /// <summary>
        /// Gets the URL of the request.
        /// </summary>
        /// <value>The URL.</value>
        Uri Url { get; }

        /// <summary>
        /// Gets the remote end point of the request.
        /// </summary>
        /// <value>
        /// The remote IP address and port of the originator of the request.
        /// </value>
        IPEndPoint RemoteEndPoint { get; }

        /// <summary>
        /// Gets all headers of the request.
        /// </summary>
        /// <value>All headers (key only) of this request.</value>
        IEnumerable<string> Headers { get; }

        /// <summary>
        /// Gets the value of a request header.
        /// </summary>
        /// <returns>The header value.</returns>
        /// <param name="header">
        /// Name of the header that should be obtained.
        /// </param>
        string GetHeaderValue(string header);

        /// <summary>
        /// Gets the HTTP request body stream.
        /// </summary>
        /// <value>HTTP request body stream.</value>
        Stream Stream { get; }
    }
}