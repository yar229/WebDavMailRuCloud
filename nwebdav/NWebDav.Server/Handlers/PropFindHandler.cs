using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Threading.Tasks;
using System.Xml.Linq;

using NWebDav.Server.Helpers;
using NWebDav.Server.Http;
using NWebDav.Server.Props;
using NWebDav.Server.Stores;

namespace NWebDav.Server.Handlers
{
    public class PropFindHandler : IRequestHandler
    {
        private struct PropertyEntry
        {
            public Uri Uri { get; }
            public IStoreItem Entry { get; }

            public PropertyEntry(Uri uri, IStoreItem entry)
            {
                Uri = uri;
                Entry = entry;
            }
        }

        [Flags]
        public enum PropertyMode
        {
            None = 0,
            PropertyNames = 1,
            AllProperties = 2,
            SelectedProperties = 4
        }

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
                response.SendResponse(DavStatusCode.NotFound);
                return true;
            }

            // Check if the entry is a collection
            var topCollection = topEntry as IStoreCollection;
            if (topCollection != null)
            {
                // Determine depth
                var depth = request.GetDepth();

                // Check if the collection supports Infinite depth for properties
                if (depth > 1 && !topCollection.AllowInfiniteDepthProperties)
                {
                    response.SendResponse(DavStatusCode.Forbidden, "Not allowed to obtain properties with infinite depth.");
                    return true;
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
                // Create the property
                var xResponse = new XElement(WebDavNamespaces.DavNs + "response",
                    new XElement(WebDavNamespaces.DavNs + "href", entry.Uri.AbsoluteUri));

                // Create tags for property values
                var xPropStatValues = new XElement(WebDavNamespaces.DavNs + "propstat");
                var xProp = new XElement(WebDavNamespaces.DavNs + "prop");
                xPropStatValues.Add(xProp);

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
                        var xPropStatErrors = new XElement(WebDavNamespaces.DavNs + "propstat");
                        var addedProperties = new List<XName>();
                        if ((propertyMode & PropertyMode.AllProperties) != 0)
                        {
                            foreach (var propertyName in propertyManager.Properties.Where(p => !p.IsExpensive).Select(p => p.Name))
                                AddProperty(httpContext, xProp, xPropStatErrors, propertyManager, entry.Entry, propertyName, addedProperties);
                        }

                        if ((propertyMode & PropertyMode.SelectedProperties) != 0)
                        {
                            foreach (var propertyName in propertyList)
                                AddProperty(httpContext, xProp, xPropStatErrors, propertyManager, entry.Entry, propertyName, addedProperties);
                        }

                        // Add the values (if any)
                        if (xPropStatValues.HasElements)
                            xResponse.Add(xPropStatValues);

                        // Add the errors (if any)
                        if (xPropStatErrors.HasElements)
                            xResponse.Add(xPropStatErrors);
                    }
                }

                // Add the status
                xPropStatValues.Add(new XElement(WebDavNamespaces.DavNs + "status", "HTTP/1.1 200 OK"));

                // Add the property
                xMultiStatus.Add(xResponse);
            }

            // Stream the document
            await response.SendResponseAsync(DavStatusCode.MultiStatus, xDocument).ConfigureAwait(false);

            // Finished writing
            return true;
        }

        private void AddProperty(IHttpContext httpContext, XElement xPropStatValues, XElement xPropStatErrors, IPropertyManager propertyManager, IStoreItem item, XName propertyName, IList<XName> addedProperties)
        {
            if (!addedProperties.Contains(propertyName))
            {
                try
                {
                    addedProperties.Add(propertyName);
                    var value = propertyManager.GetProperty(httpContext, item, propertyName);
                    if (value is XElement)
                    {
                        xPropStatValues.Add(new XElement(propertyName, (XElement)value));
                    }
                    else if (value is IEnumerable<XElement>)
                    {
                        xPropStatValues.Add(new XElement(propertyName, ((IEnumerable<XElement>)value).Cast<object>().ToArray()));
                    }
                    else
                    {
                        xPropStatValues.Add(new XElement(propertyName, value));
                    }
                }
                catch (Exception)
                {
                    // TODO
                }
            }
        }

        private PropertyMode GetRequestedProperties(IHttpRequest request, IList<XName> properties)
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
                else if (xProp.Name == WebDavNamespaces.DavNs + "propall")
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

        private async Task AddEntriesAsync(IStoreCollection collection, int depth, IHttpContext httpContext, Uri uri, IList<PropertyEntry> entries)
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



