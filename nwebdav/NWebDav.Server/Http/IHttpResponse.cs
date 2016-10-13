using System;
using System.IO;

namespace NWebDav.Server.Http
{
    /// <summary>
    /// HTTP response interface
    /// </summary>
    public interface IHttpResponse
    {
        /// <summary>
        /// Gets or sets the HTTP status code of the response.
        /// </summary>
        /// <value>HTTP response to the matching request.</value>
        int Status { get; set; }

        /// <summary>
        /// Gets or sets the HTTP status description of the response.
        /// </summary>
        /// <value>The HTTP status description.</value>
        /// <remarks>
        /// If this value is not set, then it will automatically determine
        /// the HTTP status description based upon the <see cref="Status"/>
        /// value.
        /// </remarks>
        string StatusDescription { get; set; }

        /// <summary>
        /// Sets a header to a specific value.
        /// </summary>
        /// <param name="header">Name of the header.</param>
        /// <param name="value">Value of the header.</param>
        void SetHeaderValue(string header, string value);

        /// <summary>
        /// Gets the stream that represents the response body.
        /// </summary>
        /// <value>Response body stream.</value>
        /// <remarks>
        /// It's important not to write to the stream, until the status,
        /// status description and all headers have been written. Most
        /// implementations cannot handle setting headers after the
        /// stream is written (i.e. Mono).
        /// </remarks>
        Stream Stream { get; }
    }
}