using System;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

using NWebDav.Server.Http;

namespace NWebDav.Server.Helpers
{
    /// <summary>
    /// Helper methods for <see cref="IHttpResponse"/> objects.
    /// </summary>
    public static class ResponseHelper
    {
#if DEBUG
        private static readonly NWebDav.Server.Logging.ILogger s_log = NWebDav.Server.Logging.LoggerFactory.CreateLogger(typeof(ResponseHelper));
#endif
        private static readonly UTF8Encoding s_utf8Encoding = new UTF8Encoding(false);  // Suppress BOM (not compatible with WebDrive)

        /// <summary>
        /// Set status of the HTTP response.
        /// </summary>
        /// <param name="response">
        /// The HTTP response that should be changed.
        /// </param>
        /// <param name="statusCode">
        /// WebDAV status code that should be set.
        /// </param>
        /// <param name="statusDescription">
        /// The human-readable WebDAV status description. If no status
        /// description is set (or <see langword="null"/>), then the
        /// default status description is written. 
        /// </param>
        /// <remarks>
        /// Not all HTTP infrastructures allow to set the status description,
        /// so it should only be used for informational purposes.
        /// </remarks>
        public static void SetStatus(this IHttpResponse response, DavStatusCode statusCode, string statusDescription = null)
        {
            // Set the status code and description
            response.Status = (int)statusCode;
            response.StatusDescription = statusDescription ?? statusCode.GetStatusDescription();
        }

        /// <summary>
        /// Send an HTTP response with an XML body content.
        /// </summary>
        /// <param name="response">
        /// The HTTP response that needs to be sent.
        /// </param>
        /// <param name="statusCode">
        /// WebDAV status code that should be set.
        /// </param>
        /// <param name="xDocument">
        /// XML document that should be sent as the body of the message.
        /// </param>
        /// <returns>
        /// A task that represents the asynchronous response send.
        /// </returns>
        public static async Task SendResponseAsync(this IHttpResponse response, DavStatusCode statusCode, XDocument xDocument)
        {
            // Make sure an XML document is specified
            if (xDocument == null)
                throw new ArgumentNullException(nameof(xDocument));

            // Make sure the XML document has a root node
            if (xDocument.Root == null)
                throw new ArgumentException("The specified XML document doesn't have a root node", nameof(xDocument));

            // Set the response
            response.SetStatus(statusCode);

            // Obtain the result as an XML document
            using (var ms = new MemoryStream())
            {
                using (var xmlWriter = XmlWriter.Create(ms, new XmlWriterSettings
                {
                    OmitXmlDeclaration = false,
#if DEBUG
                    Indent = true,
#else
                    Indent = false,
#endif
                    Encoding = s_utf8Encoding,
                }))
                {
                    // Add the namespaces (Win7 WebDAV client requires them like this)
                    xDocument.Root.SetAttributeValue(XNamespace.Xmlns + WebDavNamespaces.DavNsPrefix, WebDavNamespaces.DavNs);
                    xDocument.Root.SetAttributeValue(XNamespace.Xmlns + WebDavNamespaces.Win32NsPrefix, WebDavNamespaces.Win32Ns);

                    // Write the XML document to the stream
                    xDocument.WriteTo(xmlWriter);
                }

                // Flush
                ms.Flush();
#if DEBUG
                // Dump the XML document to the logging
                if (s_log.IsLogEnabled(NWebDav.Server.Logging.LogLevel.Debug))
                {
                    // Reset stream and write the stream to the result
                    ms.Seek(0, SeekOrigin.Begin);

                    var reader = new StreamReader(ms);
                    s_log.Log(NWebDav.Server.Logging.LogLevel.Debug, () => reader.ReadToEnd());
                }
#endif
                // Set content type/length
                response.SetHeaderValue("Content-Type", "text/xml; charset=\"utf-8\"");
                response.SetHeaderValue("Content-Length", ms.Position.ToString(CultureInfo.InvariantCulture));

                // Reset stream and write the stream to the result
                ms.Seek(0, SeekOrigin.Begin);
                await ms.CopyToAsync(response.Stream).ConfigureAwait(false);
            }
        }
    }
}
