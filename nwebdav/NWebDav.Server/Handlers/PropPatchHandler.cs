using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;

using NWebDav.Server.Helpers;
using NWebDav.Server.Http;
using NWebDav.Server.Stores;

namespace NWebDav.Server.Handlers
{
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
                    var statusText = $"HTTP/1.1 {(int)Result} {DavStatusCodeHelper.GetStatusDescription(Result)}";
                    return new XElement(WebDavNamespaces.DavNs + "propstat",
                        new XElement(WebDavNamespaces.DavNs + "prop", new XElement(Name)),
                        new XElement(WebDavNamespaces.DavNs + "status", statusText));
                }
            }

            public IList<PropSet> PropertySetters { get; } = new List<PropSet>();

            public PropSetCollection(IHttpRequest request)
            {
                // Create an XML document from the stream
                var xDoc = request.LoadXmlDocument();

                // The document should contain a 'propertyupdate' root element
                var xRoot = xDoc.Root;
                if (xRoot?.Name != WebDavNamespaces.DavNs + "propertyupdate")
                    throw new Exception("Invalid root element (expected 'propertyupdate')");

                // Check all descendants
                foreach (var xElement in xRoot.Elements())
                {
                    // The descendant should be a 'set' or 'remove' entry
                    if (xElement.Name != WebDavNamespaces.DavNs + "set" && xElement.Name != WebDavNamespaces.DavNs + "remove")
                        throw new Exception("Expected 'set' or 'remove' entry");

                    // Obtain the properties
                    foreach (var xProperty in xElement.Descendants(WebDavNamespaces.DavNs + "prop"))
                    {
                        // Determine the actual property element
                        var xActualProperty = xProperty.Elements().FirstOrDefault();
                        if (xActualProperty != null)
                        {
                            // Determine the new property value
                            object newValue;
                            if (xElement.Name == WebDavNamespaces.DavNs + "set")
                            {
                                // If the descendant is XML, then use the XElement, otherwise use the string
                                newValue = xActualProperty.HasElements ? (object)xActualProperty.Elements().FirstOrDefault() : xActualProperty.Value;
                            }
                            else
                            {
                                newValue = null;
                            }

                            // Add the property
                            PropertySetters.Add(new PropSet(xActualProperty.Name, newValue));
                        }
                    }
                }
            }

            public XElement GetXmlMultiStatus(Uri uri)
            {
                var xResponse = new XElement(WebDavNamespaces.DavNs + "response", new XElement(WebDavNamespaces.DavNs + "href", uri));
                var xMultiStatus = new XElement(WebDavNamespaces.DavNs + "multistatus", xResponse);
                foreach (var result in PropertySetters.Where(ps => ps.Result != DavStatusCode.Ok))
                    xResponse.Add(result.GetXmlResponse());
                return xMultiStatus;
            }
        }

        public async Task<bool> HandleRequestAsync(IHttpContext httpContext, IStore store)
        {
            // Obtain request and response
            var request = httpContext.Request;
            var response = httpContext.Response;

            // Obtain item
            var item = await store.GetItemAsync(request.Url, httpContext).ConfigureAwait(false);
            if (item == null)
            {
                response.SendResponse(DavStatusCode.NotFound);
                return true;
            }

            // Read the property set/remove items from the request
            PropSetCollection propSetCollection;
            try
            {
                // Create an XML document from the stream
                propSetCollection = new PropSetCollection(request);
            }
            catch (Exception)
            {
                response.SendResponse(DavStatusCode.BadRequest);
                return true;
            }

            // Scan each property
            foreach (var propSet in propSetCollection)
            {
                // Set the property
                DavStatusCode result;
                try
                {
                    result = item.PropertyManager.SetProperty(httpContext, item, propSet.Name, propSet.Value);
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
