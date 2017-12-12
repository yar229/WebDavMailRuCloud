using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;

using NWebDav.Server.Helpers;
using NWebDav.Server.Http;
using NWebDav.Server.Logging;
using NWebDav.Server.Props;
using NWebDav.Server.Stores;

namespace NWebDav.Server.Handlers
{
    /// <summary>
    /// Implementation of the PROPFIND method.
    /// </summary>
    /// <remarks>
    /// The specification of the WebDAV PROPFIND method can be found in the
    /// <see href="http://www.webdav.org/specs/rfc2518.html#METHOD_PROPFIND">
    /// WebDAV specification
    /// </see>.
    /// </remarks>
    public class PropFindHandler : IRequestHandler
    {
        private struct PropertyEntry
        {
            public WebDavUri Uri { get; }
            public IStoreItem Entry { get; }

            public PropertyEntry(WebDavUri uri, IStoreItem entry)
            {
                Uri = uri;
                Entry = entry;
            }
        }

        [Flags]
        private enum PropertyMode
        {
            None = 0,
            PropertyNames = 1,
            AllProperties = 2,
            SelectedProperties = 4
        }

        private static readonly ILogger s_log = LoggerFactory.CreateLogger(typeof(PropFindHandler));

        /// <summary>
        /// Handle a PROPFIND request.
        /// </summary>
        /// <param name="httpContext">
        /// The HTTP context of the request.
        /// </param>
        /// <param name="store">
        /// Store that is used to access the collections and items.
        /// </param>
        /// <returns>
        /// A task that represents the asynchronous PROPFIND operation. The task
        /// will always return <see langword="true"/> upon completion.
        /// </returns>
        public async Task<bool> HandleRequestAsync(IHttpContext httpContext, IStore store)
        {
            // Obtain request and response
            var request = httpContext.Request;
            var response = httpContext.Response;

            // Determine the list of properties that need to be obtained
            var propertyList = new List<XName>();
            var propertyMode = GetRequestedProperties(request, propertyList);

            // Generate the list of items from which we need to obtain the properties
            var entries = new List<PropertyEntry>();

            // Obtain entry
            var topEntry = await store.GetItemAsync(request.Url, httpContext).ConfigureAwait(false);
            if (topEntry == null)
            {
                response.SetStatus(DavStatusCode.NotFound);
                return true;
            }

            // Check if the entry is a collection
            var topCollection = topEntry as IStoreCollection;
            if (topCollection != null)
            {
                // Determine depth
                var depth = request.GetDepth();

                // Check if the collection supports Infinite depth for properties
                if (depth > 1)
                {
                    switch (topCollection.InfiniteDepthMode)
                    {
                        case InfiniteDepthMode.Rejected:
                            response.SetStatus(DavStatusCode.Forbidden, "Not allowed to obtain properties with infinite depth.");
                            return true;
                        case InfiniteDepthMode.Assume0:
                            depth = 0;
                            break;
                        case InfiniteDepthMode.Assume1:
                            depth = 1;
                            break;
                    }
                }

                // Add all the entries
                await AddEntriesAsync(topCollection, depth, httpContext, request.Url, entries).ConfigureAwait(false);
            }
            else
            {
                // It should be an item, so just use this item
                entries.Add(new PropertyEntry(request.Url, topEntry));
            }

            // Obtain the status document
            var xMultiStatus = new XElement(WebDavNamespaces.DavNs + "multistatus");
            var xDocument = new XDocument(xMultiStatus);

            // Add all the properties
            foreach (var entry in entries)
            {
                // we need encoded path as it is in original url (Far does not show items with spaces)
                string href = entry.Uri.PathEncoded;

                // fusedav 0.2 differs files from folders using ending '/'
                bool isCollection = entry.Entry is IStoreCollection;
                href = isCollection
                    ? href.EndsWith("/") ? href : href + "/"
                    : href.TrimEnd('/');

                // Create the property
                var xResponse = new XElement(WebDavNamespaces.DavNs + "response",
                    new XElement(WebDavNamespaces.DavNs + "href", href));

                // Create tags for property values
                var xPropStatValues = new XElement(WebDavNamespaces.DavNs + "propstat");
                var xProp = new XElement(WebDavNamespaces.DavNs + "prop");
                xPropStatValues.Add(xProp);

                var xPropStatValues404 = new XElement(WebDavNamespaces.DavNs + "propstat");
                var xProp404 = new XElement(WebDavNamespaces.DavNs + "prop");
                xPropStatValues404.Add(xProp404);

                var xPropStatValues500 = new XElement(WebDavNamespaces.DavNs + "propstat");
                var xProp500 = new XElement(WebDavNamespaces.DavNs + "prop");
                xPropStatValues500.Add(xProp500);

                // Check if the entry supports properties
                var propertyManager = entry.Entry.PropertyManager;
                if (propertyManager != null)
                {
                    // Handle based on the property mode
                    if (propertyMode == PropertyMode.PropertyNames)
                    {
                        // Add all properties
                        foreach (var property in propertyManager.Properties)
                            xPropStatValues.Add(new XElement(property.Name));

                        // Add the values
                        xResponse.Add(xPropStatValues);
                    }
                    else
                    {
                        var addedProperties = new List<XName>();
                        if ((propertyMode & PropertyMode.AllProperties) != 0)
                        {
                            foreach (var propertyName in propertyManager.Properties.Where(p => !p.IsExpensive).Select(p => p.Name))
                                await AddPropertyAsync(httpContext, xProp, xProp404, xProp500, propertyManager, entry.Entry, propertyName, addedProperties).ConfigureAwait(false);
                        }

                        if ((propertyMode & PropertyMode.SelectedProperties) != 0)
                        {
                            foreach (var propertyName in propertyList)
                                await AddPropertyAsync(httpContext, xProp, xProp404, xProp500, propertyManager, entry.Entry, propertyName, addedProperties).ConfigureAwait(false);
                        }

                        // Add the values (if any)
                        if (xProp.HasElements)
                            xResponse.Add(xPropStatValues);
                        if (xProp404.HasElements)
                            xResponse.Add(xPropStatValues404);
                        if (xProp500.HasElements)
                            xResponse.Add(xPropStatValues500);
                    }
                }

                // Add the status
                xPropStatValues.Add(new XElement(WebDavNamespaces.DavNs + "status", "HTTP/1.1 200 OK"));
                xPropStatValues404.Add(new XElement(WebDavNamespaces.DavNs + "status", "HTTP/1.1 404 Not Found"));
                xPropStatValues500.Add(new XElement(WebDavNamespaces.DavNs + "status", "HTTP/1.1 500 Internal server error"));

                // Add the property
                xMultiStatus.Add(xResponse);
            }

            // Stream the document
            await response.SendResponseAsync(DavStatusCode.MultiStatus, xDocument).ConfigureAwait(false);

            // Finished writing
            return true;
        }

        private async Task AddPropertyAsync(IHttpContext httpContext, XElement xPropValues, XElement xProp404Values, XElement xProp500Values, IPropertyManager propertyManager, IStoreItem item, XName propertyName, IList<XName> addedProperties)
        {
            if (!addedProperties.Contains(propertyName))
            {
                addedProperties.Add(propertyName);
                try
                {
                    // Check if the property is supported
                    if (propertyManager.Properties.Any(p => p.Name == propertyName))
                    {
                        var value = await propertyManager.GetPropertyAsync(httpContext, item, propertyName).ConfigureAwait(false);
                        if (value is IEnumerable<XElement>)
                            value = ((IEnumerable<XElement>)value).Cast<object>().ToArray();

                        //xPropValues.Add(new XElement(WebDavNamespaces.DavNs + "prop", new XElement(propertyName, value)));
                        xPropValues.Add(new XElement(propertyName, value));
                    }

                    // disable warning cause of too much junk log messages
                    //else
                    //{
                    //    s_log.Log(LogLevel.Warning, () => $"Property {propertyName} is not supported on item {item.Name}.");
                    //    xProp404Values.Add(new XElement(propertyName, ""));
                    //}
                }
                catch (Exception exc)
                {
                    s_log.Log(LogLevel.Error, () => $"Property {propertyName} on item {item.Name} raised an exception.", exc);
                    xProp500Values.Add(new XElement(propertyName, ""));
                }
            }
        }

        private static PropertyMode GetRequestedProperties(IHttpRequest request, ICollection<XName> properties)
        {
            // Create an XML document from the stream
            var xDocument = request.LoadXmlDocument();
            if (xDocument?.Root == null || xDocument.Root.Name != WebDavNamespaces.DavNs + "propfind")
                return PropertyMode.AllProperties;

            // Obtain the propfind node
            var xPropFind = xDocument.Root;

            // If there is no child-node, then return all properties
            var xProps = xPropFind.Elements();
            if (!xProps.Any())
                return PropertyMode.AllProperties;

            // Add all entries to the list
            var propertyMode = PropertyMode.None;
            foreach (var xProp in xPropFind.Elements())
            {
                // Check if we should fetch all property names
                if (xProp.Name == WebDavNamespaces.DavNs + "propname")
                {
                    propertyMode = PropertyMode.PropertyNames;
                }
                else if (xProp.Name == WebDavNamespaces.DavNs + "allprop")
                {
                    propertyMode = PropertyMode.AllProperties;
                }
                else if (xProp.Name == WebDavNamespaces.DavNs + "include")
                {
                    // Include properties
                    propertyMode = PropertyMode.AllProperties | PropertyMode.SelectedProperties;

                    // Include all specified properties
                    foreach (var xSubProp in xProp.Elements())
                        properties.Add(xSubProp.Name);
                }
                else
                {
                    propertyMode = PropertyMode.SelectedProperties;

                    // Include all specified properties
                    foreach (var xSubProp in xProp.Elements())
                        properties.Add(xSubProp.Name);
                }
            }

            return propertyMode;
        }

        private async Task AddEntriesAsync(IStoreCollection collection, int depth, IHttpContext httpContext, WebDavUri uri, IList<PropertyEntry> entries)
        {
            // Add the collection to the list
            entries.Add(new PropertyEntry(uri, collection));

            // If we have enough depth, then add the childs
            if (depth > 0)
            {
                // Add all child collections
                foreach (var childEntry in await collection.GetItemsAsync(httpContext).ConfigureAwait(false))
                {
                    var subUri = UriHelper.Combine(uri, childEntry.Name);
                    var subCollection = childEntry as IStoreCollection;
                    if (subCollection != null)
                        await AddEntriesAsync(subCollection, depth - 1, httpContext, subUri, entries).ConfigureAwait(false);
                    else
                        entries.Add(new PropertyEntry(subUri, childEntry));
                }
            }
        }
    }
}


