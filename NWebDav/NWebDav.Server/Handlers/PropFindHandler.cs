using System;
using System.Collections.Generic;
using System.Diagnostics;
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
            var propertyMode = await GetRequestedPropertiesAsync(request, propertyList).ConfigureAwait(false);

            // Generate the list of items from which we need to obtain the properties
            var entries = new List<PropertyEntry>();

            // Obtain entry
            var topEntry = await store.GetItemAsync(request.Url, httpContext).ConfigureAwait(false);
            switch (topEntry)
            {
                case null:
                    response.SetStatus(DavStatusCode.NotFound);
                    return true;
                // Check if the entry is a collection
                case IStoreCollection topCollection:
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
                    break;
                }
                default:
                    // It should be an item, so just use this item
                    entries.Add(new PropertyEntry(request.Url, topEntry));
                    break;
            }

            var sw = Stopwatch.StartNew();

                // Obtain the status document
            var xMultiStatus = new XElement(WebDavNamespaces.DavNsMultiStatus);
            var xDocument = new XDocument(xMultiStatus);

            object locker = new object();
            var degree = Environment.ProcessorCount > 1 && entries.Count > 5_000 ? Environment.ProcessorCount - 1 : 1;

            // Add all the properties
            //foreach (var entry in entries)
            entries
                //.AsParallel()
                //.WithDegreeOfParallelism(degree)
                //.ForAll(async entry =>
                .ForEach(async entry =>
                {
                    // we need encoded path as it is in original url (Far does not show items with spaces)
                    string href = entry.Uri.PathEncoded;

                    // fusedav 0.2 differs files from folders using ending '/'
                    //bool isCollection = entry.Entry is IStoreCollection;
                    //href = isCollection
                    //    ? href.EndsWith("/") ? href : href + "/"
                    //    : href.TrimEnd('/');

                    // Create the property
                    var xResponse = new XElement(WebDavNamespaces.DavNsResponse,
                        new XElement(WebDavNamespaces.DavNsHref, href));

                    // Create tags for property values
                    var xPropStatValues = new XElement(WebDavNamespaces.DavNsPropStat);
                    //YaR: add xProp once and do not check later
                    var xProp = new XElement(WebDavNamespaces.DavNsProp);
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
                            //var addedProperties = new HashSet<XName>(); //SortedSet<XName>((z) => { return true;} );  //ListDictionary(); //<string, XName>();
                            if ((propertyMode & PropertyMode.AllProperties) != 0)
                            {
                                foreach (var propertyName in propertyManager.Properties.Where(p => !p.IsExpensive).Select(p => p.Name))
                                    await AddPropertyAsync(httpContext, xResponse, xProp, propertyManager, entry.Entry, propertyName, null).ConfigureAwait(false);
                            }

                            if ((propertyMode & PropertyMode.SelectedProperties) != 0)
                            {
                                foreach (var propertyName in propertyList)
                                    await AddPropertyAsync(httpContext, xResponse, xProp, propertyManager, entry.Entry, propertyName, null).ConfigureAwait(false);
                            }

                            // Add the values (if any)
                            if (xPropStatValues.HasElements)
                                xResponse.Add(xPropStatValues);
                        }
                    }

                    // Add the status
                    xPropStatValues.Add(new XElement(WebDavNamespaces.DavNsStatus, "HTTP/1.1 200 OK"));

                    lock (locker)
                    {
                        // Add the property
                        xMultiStatus.Add(xResponse);
                    }

                });

            var elapsed = sw.ElapsedMilliseconds;
            s_log.Log(LogLevel.Debug, () => $"Response XML generation {elapsed} ms");

            // Stream the document
            await response.SendResponseAsync(DavStatusCode.MultiStatus, xDocument).ConfigureAwait(false);

            sw.Stop();
            s_log.Log(LogLevel.Debug, () => $"Response XML send total {sw.ElapsedMilliseconds} ms");


            // Finished writing
            return true;
        }

        private static async Task AddPropertyAsync(IHttpContext httpContext, XElement xResponse, XElement xProp, IPropertyManager propertyManager, IStoreItem item, XName propertyName, HashSet<XName> addedProperties)
        {
            if (addedProperties == null || addedProperties.Add(propertyName)) //YaR: do not check if added if we don't want this
            {
                try
                {
                    // Check if the property is supported
                    // YaR: optimize //if (propertyManager.Properties.Any(p => p.Name == propertyName))

                    var value = await propertyManager.TryGetPropertyAsync(httpContext, item, propertyName).ConfigureAwait(false);

                    if (value.IsExists)
                    {
                        //YaR: can't catch what that mean
                        //if (value is IEnumerable<XElement>)
                        //    value = ((IEnumerable<XElement>) value).Cast<object>().ToArray();

                        //YaR: xProp already added
                        //// Make sure we use the same 'prop' tag to add all properties
                        //var xProp = xPropStatValues.Element(WebDavNamespaces.DavNsProp);
                        //if (xProp == null)
                        //{
                        //    xProp = new XElement(WebDavNamespaces.DavNsProp);
                        //    xPropStatValues.Add(xProp);
                        //}

                        //xProp.Add (new XElement(propertyName, value.Value));
                        //xProp.Add(new XStreamingElement(propertyName, value.Value));
                        xProp.Add(GetPropertyXElement(propertyName, value.Value));
                    }
                    else
                    {
                        //spam on each file...
                        //s_log.Log(LogLevel.Warning, () => $"Property {propertyName} is not supported on item {item.Name}.");

                        xResponse.Add(new XElement(WebDavNamespaces.DavNsPropStat,
                            new XElement(WebDavNamespaces.DavNsProp, GetPropertyXElement(propertyName, null)),
                            DavNsStatus404XElement,
                            new XElement(WebDavNamespaces.DavNsResponseDescription, $"Property {propertyName} is not supported.")));
                    }
                }
                catch (Exception exc)
                {
                    s_log.Log(LogLevel.Error, () => $"Property {propertyName} on item {item.Name} raised an exception.", exc);
                    xResponse.Add(new XElement(WebDavNamespaces.DavNsPropStat,
                        new XElement(WebDavNamespaces.DavNsProp, GetPropertyXElement(propertyName, null)),
                        DavNsStatus500XElement,
                        new XElement(WebDavNamespaces.DavNsResponseDescription, $"Property {propertyName} on item {item.Name} raised an exception.")));
                }
            }
        }

        private static readonly XElement DavNsStatus404XElement = new(WebDavNamespaces.DavNsStatus, "HTTP/1.1 404 Not Found");
        private static readonly XElement DavNsStatus500XElement = new (WebDavNamespaces.DavNsStatus, "HTTP/1.1 500 Internal server error");

        private static XStreamingElement GetPropertyXElement(XName name, object value)
        {
            //return new XStreamingElement(name, value);

            if (!Properties2Cache.Contains(name))
                return new XStreamingElement(name, value);

            if (PropertyCache.TryGetValue(name, out var vals))
            {
                if (null == value)
                    return vals.NullElement ?? (vals.NullElement = new XStreamingElement(name, null));

                if (vals.Dict.TryGetValue(value, out var xval))
                    return xval;

                var xvalnew = new XStreamingElement(name, value);
                vals.Dict.Add(value, xvalnew);

                return xvalnew;
            }
            else
            {
                vals = new() { Dict = new Dictionary<object, XStreamingElement>() };
                var xval = new XStreamingElement(name, value);
                if (value == null)
                    vals.NullElement = xval;
                else
                    vals.Dict.Add(value, xval);

                PropertyCache.Add(name, vals);

                return xval;
            }
        }
        private static readonly Dictionary<XName, (Dictionary<object, XStreamingElement> Dict, XStreamingElement NullElement)> PropertyCache = new();

        private static readonly HashSet<XName> Properties2Cache = new()
        {
            //"resourcetype", 
            WebDavNamespaces.DavNsCollection,
            WebDavNamespaces.DavNsIsReadonly,
            WebDavNamespaces.DavNsLockDiscovery,
            WebDavNamespaces.DavNsSupportedLock,
            WebDavNamespaces.DavNsIsFolder,
            WebDavNamespaces.DavNsIsHidden,
            WebDavNamespaces.DavNsIsStructuredDocument,
            WebDavNamespaces.DavNsHasSubs,
            WebDavNamespaces.DavNsNoSubs,
            WebDavNamespaces.DavNsReserved,
            WebDavNamespaces.DavNsGetContentType,
            WebDavNamespaces.Win32NsWin32FileAttributes
        };

        private static async Task<PropertyMode> GetRequestedPropertiesAsync(IHttpRequest request, ICollection<XName> properties)
        {
            // Create an XML document from the stream
            var xDocument = await request.LoadXmlDocumentAsync().ConfigureAwait(false);
            if (xDocument?.Root == null || xDocument.Root.Name != WebDavNamespaces.DavNsPropFind)
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
                if (xProp.Name == WebDavNamespaces.DavNsPropName)
                {
                    propertyMode = PropertyMode.PropertyNames;
                }
                else if (xProp.Name == WebDavNamespaces.DavNsAllProp)
                {
                    propertyMode = PropertyMode.AllProperties;
                }
                else if (xProp.Name == WebDavNamespaces.DavNsInclude)
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

        private static async Task AddEntriesAsync(IStoreCollection collection, int depth, IHttpContext httpContext, WebDavUri uri, IList<PropertyEntry> entries)
        {
            // Add the collection to the list
            entries.Add(new PropertyEntry(uri, collection));

            // If we have enough depth, then add the children
            if (depth > 0)
            {
                // Add all child collections
                foreach (var childEntry in await collection.GetItemsAsync(httpContext).ConfigureAwait(false))
                {
                    var subUri = UriHelper.Combine(uri, childEntry.Name);
                    if (childEntry is IStoreCollection subCollection)
                        await AddEntriesAsync(subCollection, depth - 1, httpContext, subUri, entries).ConfigureAwait(false);
                    else
                        entries.Add(new PropertyEntry(subUri, childEntry));
                }
            }
        }
    }
}



