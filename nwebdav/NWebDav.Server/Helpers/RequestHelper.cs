using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Linq;

using NWebDav.Server.Http;
using NWebDav.Server.Logging;

namespace NWebDav.Server.Helpers
{
    public class SplitUri
    {
        public Uri CollectionUri { get; set; }
        public string Name { get; set; }
    }

    public class Range
    {
        public long? Start { get; set; }
        public long? End { get; set; }
        public DateTime If {get; set; }
    }

    public static class RequestHelper
    {
        private static readonly Regex s_rangeRegex = new Regex("bytes\\=(?<start>[0-9]*)-(?<end>[0-9]*)");
        private static readonly ILogger s_log = LoggerFactory.CreateLogger(typeof(RequestHelper));

        public static SplitUri SplitUri(string uri)
        {
            // Determine the offset of the name
            var slashOffset = uri.LastIndexOf('/');
            if (slashOffset == -1)
                return null;

            // Seperate name from path
            return new SplitUri
            {
                CollectionUri = new Uri(uri.Substring(0, slashOffset)),
                Name = Uri.UnescapeDataString(uri.Substring(slashOffset + 1))
            };
        }

        public static SplitUri SplitUri(Uri uri)
        {
            return SplitUri(uri.AbsoluteUri);
        }

        public static Uri GetDestinationUri(this IHttpRequest request)
        {
            // Obtain the destination
            var destinationHeader = request.GetHeaderValue("Destination");
            if (destinationHeader == null)
                return null;

            // Return the splitted Uri
            return new Uri(destinationHeader);
        }

        public static int GetDepth(this IHttpRequest request)
        {
            // Obtain the depth header (no header means infinity)
            var depthHeader = request.GetHeaderValue("Depth");
            if (depthHeader == null || depthHeader == "infinity")
                return int.MaxValue;

            // Determined depth
            int depth;
            if (!int.TryParse(depthHeader, out depth))
                return int.MaxValue;

            // Return depth
            return depth;
        }

        public static bool GetOverwrite(this IHttpRequest request)
        {
            // Get the Overwrite header
            var overwriteHeader = request.GetHeaderValue("Overwrite") ?? "T";

            // It should be set to "T" (true) or "F" (false)
            return overwriteHeader.ToUpperInvariant() == "T";
        }

        public static IList<int> GetTimeouts(this IHttpRequest request)
        {
            // Get the value of the timeout header as a string
            var timeoutHeader = request.GetHeaderValue("Timeout");
            if (string.IsNullOrEmpty(timeoutHeader))
                return null;

            // Return each item
            Func<string, int> parseTimeout = t =>
            {
                // Check for 'infinite'
                if (t == "Infinite")
                    return -1;

                // Parse the number of seconds
                int timeout;
                if (!t.StartsWith("Second-") || !int.TryParse(t.Substring(7), out timeout))
                    return 0;
                return timeout;
            };

            // Return the timeout values
            return timeoutHeader.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(parseTimeout).Where(t => t != 0).ToArray();
        }

        public static Uri GetLockToken(this IHttpRequest request)
        {
            // Get the value of the lock-token header as a string
            var lockTokenHeader = request.GetHeaderValue("Lock-Token");
            if (string.IsNullOrEmpty(lockTokenHeader))
                return null;

            // Strip the brackets from the header
            if (!lockTokenHeader.StartsWith("<") || !lockTokenHeader.EndsWith(">"))
                return null;

            // Create an Uri of the intermediate part
            return new Uri(lockTokenHeader.Substring(1, lockTokenHeader.Length - 2), UriKind.Absolute);
        }

        public static Uri GetIfLockToken(this IHttpRequest request)
        {
            // Get the value of the lock-token header as a string
            var lockTokenHeader = request.GetHeaderValue("If");
            if (string.IsNullOrEmpty(lockTokenHeader))
                return null;

            // Strip the brackets from the header
            if (!lockTokenHeader.StartsWith("(<") || !lockTokenHeader.EndsWith(">)"))
                return null;

            // Create an Uri of the intermediate part
            return new Uri(lockTokenHeader.Substring(2, lockTokenHeader.Length - 4), UriKind.Absolute);
        }

        public static Range GetRange(this IHttpRequest request)
        {
            // Get the value of the range header as a string
            var rangeHeader = request.GetHeaderValue("Range");
            if (string.IsNullOrEmpty(rangeHeader))
                return null;

            // We only support the bytes=<start>-<end> format
            var match = s_rangeRegex.Match(rangeHeader);
            if (!match.Success)
                throw new FormatException($"Illegal format for range header: {rangeHeader}");

            // Obtain the start and end
            var startText = match.Groups["start"].Value;
            var endText = match.Groups["end"].Value;
            var range = new Range
            {
                Start = !string.IsNullOrEmpty(startText) ? (long?)long.Parse(startText) : null,
                End = !string.IsNullOrEmpty(endText) ? (long?)long.Parse(endText ) : null
            };

            // Check if we also have an If-Range
            var ifRangeHeader = request.GetHeaderValue("If-Range");
            if (ifRangeHeader != null)
            {
                // Attempt to parse the date. If we don't understand the If-Range
                // then we need to return the entire file, so we will act as if no
                // range was specified at all.
                DateTime dt;
                if (!DateTime.TryParse(ifRangeHeader, out dt))
                    return null;

                // Use the date for the 'If'
                range.If = dt;
            }

            // Return the range
            return range;
        }

        public static XDocument LoadXmlDocument(this IHttpRequest request)
        {
            // If there is no input stream, then there is no XML document
            if (request.Stream == null || request.Stream == Stream.Null)
                return null;

            // If there is no data,
            var contentLengthString = request.GetHeaderValue("Content-Length");
            if (contentLengthString != null)
            {
                int contentLength;
                if (!int.TryParse(contentLengthString, out contentLength) || contentLength == 0)
                    return null;
            }

            // Obtain an XML document from the stream
            var xDocument = XDocument.Load(request.Stream);
#if DEBUG
            // Dump the XML document to the logging
            if (xDocument.Root != null && s_log.IsLogEnabled(LogLevel.Debug))
            {
                // Format the XML document as an in-memory text representation
                using (var ms = new System.IO.MemoryStream())
                {
                    using (var xmlWriter = System.Xml.XmlWriter.Create(ms, new System.Xml.XmlWriterSettings
                    {
                        OmitXmlDeclaration = false,
                        Indent = true,
                        Encoding = System.Text.Encoding.UTF8,
                    }))
                    {
                        // Write the XML document to the stream
                        xDocument.WriteTo(xmlWriter);
                    }

                    // Flush
                    ms.Flush();

                    // Reset stream and write the stream to the result
                    ms.Seek(0, System.IO.SeekOrigin.Begin);

                    // Log the XML text to the logging
                    var reader = new System.IO.StreamReader(ms);
                    s_log.Log(LogLevel.Debug, reader.ReadToEnd());
                }
            }
#endif
            // Return the XML document
            return xDocument;
        }
    }
}
