using System;
using System.IO;
using System.Net;
using System.Net.Http.Headers;
using System.Text;

namespace WebDAVSharp.Server.Adapters
{
    /// <summary>
    /// This 
    /// <see cref="IHttpListenerResponse" /> implementation wraps around a
    /// <see cref="HttpListenerResponse" /> instance.
    /// </summary>
    internal sealed class HttpListenerResponseAdapter : IHttpListenerResponse
    {
        #region Private Variables

        private readonly HttpListenerResponse _response;

        #endregion

        #region Properties
        /// <summary>
        /// Gets the internal instance that was adapted for WebDAV#.
        /// </summary>
        /// <value>
        /// The adapted instance.
        /// </value>
        public HttpListenerResponse AdaptedInstance
        {
            get
            {
                return _response;
            }
        }

        /// <summary>
        /// Gets or sets the HTTP status code to be returned to the client.
        /// </summary>
        public int StatusCode
        {
            get
            {
                return _response.StatusCode;
            }

            set
            {
                _response.StatusCode = value;
            }
        }

        /// <summary>
        /// Gets or sets a text description of the HTTP <see cref="StatusCode">status code</see> returned to the client.
        /// </summary>
        public string StatusDescription
        {
            get
            {
                return _response.StatusDescription;
            }

            set
            {
                _response.StatusDescription = value ?? string.Empty;
            }
        }

        /// <summary>
        /// Gets a <see cref="Stream" /> object to which a response can be written.
        /// </summary>
        public Stream OutputStream
        {
            get
            {
                return _response.OutputStream;
            }
        }

        /// <summary>
        /// Gets or sets the <see cref="Encoding" /> for this response's <see cref="OutputStream" />.
        /// </summary>
        public Encoding ContentEncoding
        {
            get
            {
                return _response.ContentEncoding;
            }

            set
            {
                _response.ContentEncoding = value;
            }
        }

        /// <summary>
        /// Gets or sets the number of bytes in the body data included in the response.
        /// </summary>
        public long ContentLength64
        {
            get
            {
                return _response.ContentLength64;
            }

            set
            {
                _response.ContentLength64 = value;
            }
        }

        #endregion

        #region Public Functions

        /// <summary>
        /// Initializes a new instance of the <see cref="HttpListenerResponseAdapter" /> class.
        /// </summary>
        /// <param name="response">The <see cref="HttpListenerResponse" /> to adapt for WebDAV#.</param>
        /// <exception cref="System.ArgumentNullException">Response</exception>
        /// <exception cref="ArgumentNullException"><paramref name="response" /> is <c>null</c>.</exception>
        public HttpListenerResponseAdapter(HttpListenerResponse response)
        {
        if (response == null)
            throw new ArgumentNullException("response");

        _response = response;
        }

        /// <summary>
        /// Sends the response to the client and releases the resources held by the adapted
        /// <see cref="HttpListenerResponse" /> instance.
        /// </summary>
        public void Close()
        {
            _response.Close();
        }

        /// <summary>
        /// Appends a value to the specified HTTP header to be sent with the response.
        /// </summary>
        /// <param name="name">The name of the HTTP header to append the <paramref name="value" /> to.</param>
        /// <param name="value">The value to append to the <paramref name="name" /> header.</param>
        public void AppendHeader(string name, string value)
        {
            _response.AppendHeader(name, value);
        }

        public void SetEtag(String etag)
        {
            _response.Headers.Add("ETag", etag);
        }

        public void SetLastModified(DateTime date)
        {
            _response.Headers.Add("Last-Modified", GetLastModifiedFromDate(date));
        }

        public static String GetLastModifiedFromDate(DateTime date)
        {
            return date.ToUniversalTime().ToString("ddd, dd MMM yyyy hh:mm:ss G\\MT", System.Globalization.CultureInfo.InvariantCulture);
        }

        public string DumpHeaders()
        {
            StringBuilder headers = new StringBuilder();
            headers.AppendFormat("STATUS CODE: {0}\r\n", _response.StatusCode);
            foreach (String header in _response.Headers)
            {
                headers.AppendFormat("{0}: {1}\r\n", header, _response.Headers[header]);
            }
            return headers.ToString();
        }


        #endregion
    }
}