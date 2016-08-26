using System;
using System.Linq;
using System.Net;
using System.Web;
using WebDAVSharp.Server.Adapters;
using WebDAVSharp.Server.Exceptions;
using WebDAVSharp.Server.Stores;

namespace WebDAVSharp.Server
{
    /// <summary>
    /// This class holds extension methods for various types related to WebDAV#.
    /// </summary>
    internal static class WebDavExtensions
    {
        /// <summary>
        /// Gets the Uri to the parent object.
        /// </summary>
        /// <param name="uri">The <see cref="Uri" /> of a resource, for which the parent Uri should be retrieved.</param>
        /// <returns>
        /// The parent <see cref="Uri" />.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">uri</exception>
        /// <exception cref="System.InvalidOperationException">Cannot get parent of root</exception>
        /// <exception cref="ArgumentNullException"><paramref name="uri" /> is <c>null</c>.</exception>
        /// <exception cref="InvalidOperationException"><paramref name="uri" /> has no parent, it refers to a root resource.</exception>
        public static Uri GetParentUri(this Uri uri)
        {
            if (uri == null)
                throw new ArgumentNullException("uri");
            if (uri.Segments.Length == 1)
                return uri;

            string url = uri.ToString();
            int index = url.Length - 1;
            if (url[index] == '/')
                index--;
            while (url[index] != '/')
                index--;
            return new Uri(url.Substring(0, index + 1));
        }

        public static String GetLastSegment(this Uri uri)
        {
            var segment = uri.AbsoluteUri
               .Split('\\', '/')
               .Select(s => s.Trim('\\', '/'))
               .Where(s => !String.IsNullOrWhiteSpace(s))
               .LastOrDefault();

            if (!String.IsNullOrEmpty(segment))
                segment = Uri.UnescapeDataString(segment);
            return segment;
        }

        /// <summary>
        /// Sends a simple response with a specified HTTP status code but no content.
        /// </summary>
        /// <param name="context">The <see cref="IHttpListenerContext" /> to send the response through.</param>
        /// <param name="statusCode">The HTTP status code for the response.</param>
        /// <exception cref="System.ArgumentNullException">context</exception>
        /// <exception cref="ArgumentNullException"><paramref name="context" /> is <c>null</c>.</exception>
        public static void SendSimpleResponse(this IHttpListenerContext context, int statusCode = (int)HttpStatusCode.OK)
        {
            if (context == null)
                throw new ArgumentNullException("context");

            context.Response.StatusCode = statusCode;
            context.Response.StatusDescription = HttpWorkerRequest.GetStatusDescription(statusCode);
            context.Response.Close();
        }

        /// <summary>
        /// Gets the prefix <see cref="Uri" /> that matches the specified <see cref="Uri" />.
        /// </summary>
        /// <param name="uri">The <see cref="Uri" /> to find the most specific prefix <see cref="Uri" /> for.</param>
        /// <param name="server">The 
        /// <see cref="WebDavServer" /> that hosts the WebDAV server and holds the collection
        /// of known prefixes.</param>
        /// <returns>
        /// The most specific <see cref="Uri" /> for the given <paramref name="uri" />.
        /// </returns>
        /// <exception cref="WebDAVSharp.Server.Exceptions.WebDavInternalServerException">Unable to find correct server root</exception>
        /// <exception cref="WebDavInternalServerException"><paramref name="uri" /> specifies a <see cref="Uri" /> that is not known to the <paramref name="server" />.</exception>
        public static Uri GetPrefixUri(this Uri uri, WebDavServer server)
        {
            string url = uri.ToString();


            var pfs = server.Listener.Prefixes.ToList();
            string exactPrefix = pfs
                .FirstOrDefault(item => url.StartsWith(item.TrimEnd('/'), StringComparison.InvariantCultureIgnoreCase));

            if (!string.IsNullOrEmpty(exactPrefix))
            {
                return new Uri(exactPrefix);
            }

            Uri wildcardPrefix = GetPrefixWithWildCard(uri, server);

            if (wildcardPrefix != null)
            {
                return wildcardPrefix;
            }

            throw new WebDavInternalServerException("Unable to find correct server root");
        }

        private static String[] AllIpWildChards = new[] { "*", "+" };

        private static Uri GetPrefixWithWildCard(Uri uri, WebDavServer server)
        {
            foreach (var wc in AllIpWildChards)
            {
                string wildcardUrl = new UriBuilder(uri) { Host = "WebDAVSharpSpecialHostTag" }
               .ToString().Replace("WebDAVSharpSpecialHostTag", wc);

                string wildcardPrefix = server.Listener.Prefixes
                    .FirstOrDefault(item => wildcardUrl.StartsWith(item, StringComparison.OrdinalIgnoreCase));
                if (!String.IsNullOrEmpty(wildcardPrefix))
                {
                    return new Uri(wildcardPrefix.Replace("://" + wc, string.Format("://{0}", uri.Host)));
                } 
            }
            return null;
        }

        /// <summary>
        /// Retrieves a store item through the specified
        /// <see cref="Uri" /> from the
        /// specified
        /// <see cref="WebDavServer" /> and
        /// <see cref="IWebDavStore" />.
        /// </summary>
        /// <param name="uri">The <see cref="Uri" /> to retrieve the store item for.</param>
        /// <param name="server">The <see cref="WebDavServer" /> that hosts the <paramref name="store" />.</param>
        /// <param name="store">The <see cref="IWebDavStore" /> from which to retrieve the store item.</param>
        /// <returns>
        /// The retrieved store item.
        /// </returns>
        /// <exception cref="System.ArgumentNullException"><para>
        ///   <paramref name="uri" /> is <c>null</c>.</para>
        /// <para>
        ///   <paramref name="server" /> is <c>null</c>.</para>
        /// <para>
        ///   <paramref name="store" /> is <c>null</c>.</para></exception>
        /// <exception cref="WebDAVSharp.Server.Exceptions.WebDavNotFoundException">If the item was not found.</exception>
        /// <exception cref="WebDavConflictException"><paramref name="uri" /> refers to a document in a collection, where the collection does not exist.</exception>
        /// <exception cref="WebDavNotFoundException"><paramref name="uri" /> refers to a document that does not exist.</exception>
        public static IWebDavStoreItem GetItem(
            this Uri uri,
            WebDavServer server, 
            IWebDavStore store)
        {
            if (uri == null)
                throw new ArgumentNullException("uri");
            if (server == null)
                throw new ArgumentNullException("server");
            if (store == null)
                throw new ArgumentNullException("store");

            Uri prefixUri = uri.GetPrefixUri(server);
            IWebDavStoreCollection collection = store.Root;

            IWebDavStoreItem item = null;
            if (prefixUri.Segments.Length == uri.Segments.Length)
                return collection;

            string[] segments = SplitUri(uri, prefixUri);

            for (int index = 0; index < segments.Length; index++)
            {
                string segmentName = Uri.UnescapeDataString(segments[index]);

                IWebDavStoreItem nextItem = collection.GetItemByName(segmentName);
                if (nextItem == null)
                    throw new WebDavNotFoundException(String.Format("Cannot find item {0} from collection {1}", segmentName, collection.ItemPath)); //throw new WebDavConflictException();

                if (index == segments.Length - 1)
                    item = nextItem;
                else
                {
                    collection = nextItem as IWebDavStoreCollection;
                    if (collection == null)
                        throw new WebDavNotFoundException(String.Format("NextItem [{0}] is not a collection", nextItem.ItemPath));
                }
            }

            if (item == null)
                throw new WebDavNotFoundException(String.Format("Unable to find {0}", uri));

            return item;
        }

        private static string[] SplitUri(Uri uri, Uri prefixUri)
        {
            return uri.AbsoluteUri.Substring(prefixUri.AbsoluteUri.Length)
                .Split('\\', '/')
                .Select(s => s.Trim('\\', '/'))
                .Where(s => !String.IsNullOrWhiteSpace(s))
                .ToArray();
        }
    }
}