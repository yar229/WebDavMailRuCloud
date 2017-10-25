using System.IO;

namespace NWebDav.Server.Http
{
    /// <summary>
    /// HTTP response message interface.
    /// </summary>
    public interface IHttpResponse
    {
        /// <summary>
        /// Gets or sets the HTTP status code of the response.
        /// </summary>
        /// <value>HTTP response to the matching request.</value>
        /// <remarks>
        /// The WebDAV specification extends the standard HTTP status
        /// codes. These additional codes have been defined in the
        /// <seealso cref="DavStatusCode"/> enumeration. Although it is
        /// possible to return an arbitrary integer value, it is recommended
        /// to stick to the values defined in <seealso cref="DavStatusCode"/>.
        /// </remarks>
        int Status { get; set; }

        /// <summary>
        /// Gets or sets the HTTP status description of the response.
        /// </summary>
        /// <value>The HTTP status description.</value>
        /// <remarks>
        /// If this value is not set, then it will automatically determine
        /// the HTTP status description based upon the
        /// <seealso cref="Status"/> value. The description is based on the
        /// value of the 
        /// <see cref="NWebDav.Server.Helpers.DavStatusCodeAttribute"/>
        /// attribute for this status.
        /// </remarks>
        string StatusDescription { get; set; }

        /// <summary>
        /// Sets a header to a specific value.
        /// </summary>
        /// <param name="header">Name of the header.</param>
        /// <param name="value">Value of the header.</param>
        /// <remarks>
        /// The <paramref name="header"/> is case insensitive, but it is
        /// recommended to adhere to the casing as defined in the WebDAV
        /// specification.
        /// </remarks>
        void SetHeaderValue(string header, string value);

        /// <summary>
        /// Gets the stream that represents the response body.
        /// </summary>
        /// <value>Response body stream.</value>
        /// <remarks>
        /// <para>
        /// It's important not to write to the stream, until the status,
        /// status description and all headers have been written. Most
        /// implementations cannot handle setting headers after the
        /// stream is written (i.e. Mono).
        /// </para>
        /// <para>
        /// The HTTP response body stream should only support forward-only
        /// writing of the stream. The internal NWebDAV code doesn't use
        /// positioning (seeking) within the stream.
        /// </para>
        /// </remarks>
        Stream Stream { get; }
    }
}