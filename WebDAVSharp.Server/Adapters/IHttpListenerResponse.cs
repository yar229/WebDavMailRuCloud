using System;
using System.IO;
using System.Net;
using System.Net.Http.Headers;
using System.Text;

namespace WebDAVSharp.Server.Adapters
{
    /// <summary>
    /// This is an interface-version of the parts of 
    /// <see cref="HttpListenerResponse" /> that
    /// the 
    /// <see cref="WebDavServer" /> requires to operator.
    /// </summary>
    /// <remarks>
    /// The main purpose of this interface is to facilitate unit-testing.
    /// </remarks>
    public interface IHttpListenerResponse : IAdapter<HttpListenerResponse>
    {
        /// <summary>
        /// Gets or sets the HTTP status code to be returned to the client.
        /// </summary>
        /// <value>
        /// The status code.
        /// </value>
        int StatusCode
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a text description of the HTTP <see cref="StatusCode">status code</see> returned to the client.
        /// </summary>
        /// <value>
        /// The status description.
        /// </value>
        string StatusDescription
        {
            get;
            set;
        }

        /// <summary>
        /// Gets a <see cref="Stream" /> object to which a response can be written.
        /// </summary>
        /// <value>
        /// The output stream.
        /// </value>
        Stream OutputStream
        {
            get;
        }

        /// <summary>
        /// Gets or sets the <see cref="Encoding" /> for this response's <see cref="OutputStream" />.
        /// </summary>
        /// <value>
        /// The content encoding.
        /// </value>
        Encoding ContentEncoding
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the number of bytes in the body data included in the response.
        /// </summary>
        /// <value>
        /// The content length64.
        /// </value>
        long ContentLength64
        {
            get;
            set;
        }

        /// <summary>
        /// Sends the response to the client and releases the resources held by the adapted
        /// <see cref="HttpListenerResponse" /> instance.
        /// </summary>
        void Close();

        /// <summary>
        /// Appends a value to the specified HTTP header to be sent with the response.
        /// </summary>
        /// <param name="name">The name of the HTTP header to append the <paramref name="value" /> to.</param>
        /// <param name="value">The value to append to the <paramref name="name" /> header.</param>
        void AppendHeader(string name, string value);

        /// <summary>
        /// Set the Etag for this specific request.
        /// </summary>
        /// <param name="etag"></param>
        void SetEtag(String etag);

        /// <summary>
        /// Set last-modified header.
        /// </summary>
        /// <param name="date"></param>
        void SetLastModified(DateTime date);

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        System.String DumpHeaders();
    }
}