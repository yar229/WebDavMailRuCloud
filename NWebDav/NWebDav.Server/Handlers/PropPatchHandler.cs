﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;

using NWebDav.Server.Helpers;
using NWebDav.Server.Http;
using NWebDav.Server.Stores;

namespace NWebDav.Server.Handlers
{
    /// <summary>
    /// Implementation of the PROPPATCH method.
    /// </summary>
    /// <remarks>
    /// The specification of the WebDAV PROPFIND method can be found in the
    /// <see href="http://www.webdav.org/specs/rfc2518.html#METHOD_PROPPATCH">
    /// WebDAV specification
    /// </see>.
    /// </remarks>
    public class PropPatchHandler : IRequestHandler
    {
        private class PropSetCollection : List<PropSetCollection.PropSet>
        {
            public class PropSet
            {
                public XName Name { get; }
                public object Value { get; }
                public DavStatusCode Result { get; set; }

                public PropSet(XName name, object value)
                {
                    Name = name;
                    Value = value;
                }

                public XElement GetXmlResponse()
                {
                    var statusText = $"HTTP/1.1 {(int)Result} {Result.GetStatusDescription()}";
                    return new XElement(WebDavNamespaces.DavNsPropStat,
                        new XElement(WebDavNamespaces.DavNsProp, new XElement(Name)),
                        new XElement(WebDavNamespaces.DavNsStatus, statusText));
                }
            }

            private readonly IList<PropSet> _propertySetters = new List<PropSet>();

            public PropSetCollection(XElement xPropertyUpdate)
            {
                // The document should contain a 'propertyupdate' root element
                if (xPropertyUpdate == null || xPropertyUpdate.Name != WebDavNamespaces.DavNsPropertyUpdate)
                    throw new Exception("Invalid root element (expected 'propertyupdate')");

                // Check all descendants
                foreach (var xElement in xPropertyUpdate.Elements())
                {
                    // The descendant should be a 'set' or 'remove' entry
                    if (xElement.Name != WebDavNamespaces.DavNsSet && xElement.Name != WebDavNamespaces.DavNsRemove)
                        throw new Exception("Expected 'set' or 'remove' entry");

                    // Obtain the properties
                    foreach (var xProperty in xElement.Descendants(WebDavNamespaces.DavNsProp))
                    {
                        // Determine the actual property element
                        var xActualProperty = xProperty.Elements().FirstOrDefault();
                        if (xActualProperty != null)
                        {
                            // Determine the new property value
                            object newValue;
                            if (xElement.Name == WebDavNamespaces.DavNsSet)
                            {
                                // If the descendant is XML, then use the XElement, otherwise use the string
                                newValue = xActualProperty.HasElements ? (object)xActualProperty.Elements().FirstOrDefault() : xActualProperty.Value;
                            }
                            else
                            {
                                newValue = null;
                            }

                            // Add the property
                            _propertySetters.Add(new PropSet(xActualProperty.Name, newValue));
                        }
                    }
                }
            }

            public XElement GetXmlMultiStatus(WebDavUri uri)
            {
                var xResponse = new XElement(WebDavNamespaces.DavNsResponse, new XElement(WebDavNamespaces.DavNsHref, uri));
                var xMultiStatus = new XElement(WebDavNamespaces.DavNsMultiStatus, xResponse);
                foreach (var result in _propertySetters.Where(ps => ps.Result != DavStatusCode.Ok))
                    xResponse.Add(result.GetXmlResponse());
                return xMultiStatus;
            }
        }

        /// <summary>
        /// Handle a PROPPATCH request.
        /// </summary>
        /// <param name="httpContext">
        /// The HTTP context of the request.
        /// </param>
        /// <param name="store">
        /// Store that is used to access the collections and items.
        /// </param>
        /// <returns>
        /// A task that represents the asynchronous PROPPATCH operation. The task
        /// will always return <see langword="true"/> upon completion.
        /// </returns>
        public async Task<bool> HandleRequestAsync(IHttpContext httpContext, IStore store)
        {
            // Obtain request and response
            var request = httpContext.Request;
            var response = httpContext.Response;

            // Obtain item
            var item = await store.GetItemAsync(request.Url, httpContext).ConfigureAwait(false);
            if (item == null)
            {
                response.SetStatus(DavStatusCode.NotFound);
                return true;
            }

            // Read the property set/remove items from the request
            PropSetCollection propSetCollection;
            try
            {
                // Create an XML document from the stream
                var xDoc = await request.LoadXmlDocumentAsync().ConfigureAwait(false);

                // Create an XML document from the stream
                propSetCollection = new PropSetCollection(xDoc.Root);
            }
            catch (Exception)
            {
                response.SetStatus(DavStatusCode.BadRequest);
                return true;
            }

            // Scan each property
            foreach (var propSet in propSetCollection)
            {
                // Set the property
                DavStatusCode result;
                try
                {
                    result = await item.PropertyManager.SetPropertyAsync(httpContext, item, propSet.Name, propSet.Value).ConfigureAwait(false);
                }
                catch (Exception)
                {
                    result = DavStatusCode.Forbidden;
                }

                propSet.Result = result;
            }

            // Obtain the status document
            var xDocument = new XDocument(propSetCollection.GetXmlMultiStatus(request.Url));

            // Stream the document
            await response.SendResponseAsync(DavStatusCode.MultiStatus, xDocument).ConfigureAwait(false);
            return true;
        }
    }
}
