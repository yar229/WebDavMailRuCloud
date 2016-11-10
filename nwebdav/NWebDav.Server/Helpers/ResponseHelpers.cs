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
    public static class ResponseHelper
    {
#if DEBUG
        private static readonly NWebDav.Server.Logging.ILogger s_log = NWebDav.Server.Logging.LoggerFactory.CreateLogger(typeof(ResponseHelper));
#endif

        public static void SendResponse(this IHttpResponse response, DavStatusCode statusCode, string statusDescription = null)
        {
            // Set the status code and description
            response.Status = (int)statusCode;
            response.StatusDescription = statusDescription ?? DavStatusCodeHelper.GetStatusDescription(statusCode);
        }

        public static async Task SendResponseAsync(this IHttpResponse response, DavStatusCode statusCode, XDocument xDocument)
        {
            // Make sure an XML document is specified
            if (xDocument == null)
                throw new ArgumentNullException(nameof(xDocument));

            // Make sure the XML document has a root node
            if (xDocument.Root == null)
                throw new ArgumentException("The specified XML document doesn't have a root node", nameof(xDocument));

            // Set the response
            response.SendResponse(statusCode);

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
                    Encoding = Encoding.UTF8,
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
                    s_log.Log(NWebDav.Server.Logging.LogLevel.Debug, reader.ReadToEnd());
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
