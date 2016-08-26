using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Xml;

using WebDAVSharp.Server.Adapters;
using WebDAVSharp.Server.Exceptions;
using WebDAVSharp.Server.Stores;
using WebDAVSharp.Server.Stores.Locks;
using WebDAVSharp.Server.Utilities;


namespace WebDAVSharp.Server.MethodHandlers
{
    /// <summary>
    /// This class implements the <c>PROPFIND</c> HTTP method for WebDAV#.
    /// </summary>
    internal class WebDavPropfindMethodHandler : WebDavMethodHandlerBase
    {
        #region Variables

        private Uri _requestUri;
        private List<WebDavProperty> _requestedProperties;
        private IEnumerable<IWebDavStoreItem> _webDavStoreItems;

        private static List<WebDavProperty> _list = new List<WebDavProperty>
            {
                new WebDavProperty("creationdate"),
                new WebDavProperty("displayname"),
                new WebDavProperty("getcontentlength"),
                new WebDavProperty("getcontenttype"),
                new WebDavProperty("getetag"),
                new WebDavProperty("getlastmodified"),
                new WebDavProperty("resourcetype"),
                new WebDavProperty("supportedlock"),
                new WebDavProperty("ishidden")       ,
                //new WebDavProperty("getcontentlanguage"),
                //new WebDavProperty("lockdiscovery")
            };


        #endregion

        #region Properties

        /// <summary>
        /// Gets the collection of the names of the HTTP methods handled by this instance.
        /// </summary>
        /// <value>
        /// The names.
        /// </value>
        public override IEnumerable<string> Names
        {
            get
            {
                return new[]
                {
                    "PROPFIND"
                };
            }
        }

        internal IHttpListenerContext _Context;

        #endregion

        #region Functions

        /// <summary>
        /// Processes the request.
        /// </summary>
        /// <param name="server">The <see cref="WebDavServer" /> through which the request came in from the client.</param>
        /// <param name="context">The
        /// <see cref="IHttpListenerContext" /> object containing both the request and response
        /// objects to use.</param>
        /// <param name="store">The <see cref="IWebDavStore" /> that the <see cref="WebDavServer" /> is hosting.</param>
        /// <exception cref="WebDAVSharp.Server.Exceptions.WebDavUnauthorizedException"></exception>
        /// <param name="response"></param>
        /// <param name="request"></param>
        protected override void OnProcessRequest(
           WebDavServer server,
           IHttpListenerContext context,
           IWebDavStore store,
           XmlDocument request,
           XmlDocument response)
        {
            _Context = context;
            /***************************************************************************************************
             * Retreive all the information from the request
             ***************************************************************************************************/

            // Read the headers, ...
            bool isPropname = false;
            int depth = GetDepthHeader(context.Request);
            _requestUri = GetRequestUri(context.Request.Url.ToString());
            try
            {
                var webDavStoreItem = context.Request.Url.GetItem(server, store);
                _webDavStoreItems = GetWebDavStoreItems(webDavStoreItem, depth);
            }
            catch (UnauthorizedAccessException)
            {
                throw new WebDavUnauthorizedException();
            }

            // Get the XmlDocument from the request
            var requestDocument = GetXmlDocument(context.Request);

            // See what is requested
            _requestedProperties = new List<WebDavProperty>();
            if (requestDocument != null && requestDocument.DocumentElement != null)
            {
                if (requestDocument.DocumentElement.LocalName != "propfind")
                    WebDavServer.Log.Debug("PROPFIND method without propfind in xml document");
                else
                {
                    XmlNode n = requestDocument.DocumentElement.FirstChild;
                    if (n == null)
                        WebDavServer.Log.Debug("propfind element without children");
                    else
                    {
                        switch (n.LocalName)
                        {
                            case "allprop":
                                _requestedProperties = GetAllProperties();
                                break;
                            case "propname":
                                isPropname = true;
                                _requestedProperties = GetAllProperties();
                                break;
                            case "prop":
                                foreach (XmlNode child in n.ChildNodes)
                                    _requestedProperties.Add(new WebDavProperty(child.LocalName, "", child.NamespaceURI));
                                break;
                            default:
                                _requestedProperties.Add(new WebDavProperty(n.LocalName, "", n.NamespaceURI));
                                break;
                        }
                    }
                }
            }
            else
                _requestedProperties = GetAllProperties();

            /***************************************************************************************************
             * Create the body for the response
             ***************************************************************************************************/
            XmlDocument responseDoc = ResponseDocument(context, isPropname, response);

            /***************************************************************************************************
             * Send the response
             ***************************************************************************************************/
            if (WebDavServer.Log.IsDebugEnabled)
            {
                WebDavServer.Log.DebugFormat(
@"Request {0}:{1}:{2}
Request
{3}
Response:
{4}",
                context.Request.HttpMethod, context.Request.RemoteEndPoint, context.Request.Url,
                request.Beautify(), 
                response.Beautify());
            }


            SendResponse(context, response);
        }

        #region RetrieveInformation

        /// <summary>
        /// Get the URI to the location
        /// If no slash at the end of the URI, this method adds one
        /// </summary>
        /// <param name="uri">The <see cref="string" /> that contains the URI</param>
        /// <returns>
        /// The <see cref="Uri" /> that contains the given uri
        /// </returns>
        private static Uri GetRequestUri(string uri)
        {
            return new Uri(uri.EndsWith("/") ? uri : uri + "/");
        }

        /// <summary>
        /// Convert the given 
        /// <see cref="IWebDavStoreItem" /> to a 
        /// <see cref="List{T}" /> of 
        /// <see cref="IWebDavStoreItem" />
        /// This list depends on the "Depth" header
        /// </summary>
        /// <param name="iWebDavStoreItem">The <see cref="IWebDavStoreItem" /> that needs to be converted</param>
        /// <param name="depth">The "Depth" header</param>
        /// <returns>
        /// A <see cref="List{T}" /> of <see cref="IWebDavStoreItem" />
        /// </returns>
        /// <exception cref="WebDAVSharp.Server.Exceptions.WebDavConflictException"></exception>
        private HashSet<IWebDavStoreItem> GetWebDavStoreItems(IWebDavStoreItem iWebDavStoreItem, int depth)
        {
            HashSet<IWebDavStoreItem> list = new HashSet<IWebDavStoreItem>();

            //IWebDavStoreCollection
            // if the item is a collection
            IWebDavStoreCollection collection = iWebDavStoreItem as IWebDavStoreCollection;
            if (collection != null)
            {
                list.Add(collection);
                if (depth == 0)
                    return list;

                foreach (var item in collection.Items)
                {
                    list.Add(item);
                }

                return list;
            }
            // if the item is not a document, throw conflict exception
            if (!(iWebDavStoreItem is IWebDavStoreDocument))
                throw new WebDavConflictException(String.Format("Web dav item is not a document nor a collection: {0} depth {1}", iWebDavStoreItem.ItemPath, depth));

            // add the item to the list
            list.Add(iWebDavStoreItem);

            return list;
        }

        /// <summary>
        /// Reads the XML body of the 
        /// <see cref="IHttpListenerRequest" />
        /// and converts it to an 
        /// <see cref="XmlDocument" />
        /// </summary>
        /// <param name="request">The <see cref="IHttpListenerRequest" /></param>
        /// <returns>
        /// The <see cref="XmlDocument" /> that contains the request body
        /// </returns>
        private static XmlDocument GetXmlDocument(IHttpListenerRequest request)
        {
            string requestBody = "";
            try
            {
                StreamReader reader = new StreamReader(request.InputStream, Encoding.UTF8);
                requestBody = reader.ReadToEnd();
                reader.Close();

                if (!String.IsNullOrEmpty(requestBody))
                {
                    XmlDocument xmlDocument = new XmlDocument();
                    xmlDocument.LoadXml(requestBody);
                    return xmlDocument;
                }
            }
            catch (Exception ex)
            {
                WebDavServer.Log.WarnFormat("XmlDocument has not been read correctly: {0}", requestBody);
                throw new WebDavBadRequestException("Malformed XML", ex);
            }

            return new XmlDocument();
        }

        /// <summary>
        /// Adds the standard properties for an Propfind allprop request to a <see cref="List{T}" /> of <see cref="WebDavProperty" />
        /// </summary>
        /// <returns>
        /// The list with all the <see cref="WebDavProperty" />
        /// </returns>
        private static List<WebDavProperty> GetAllProperties()
        {
            return _list;
        }

        #endregion

        #region BuildResponseBody

        /// <summary>
        /// Builds the <see cref="XmlDocument" /> containing the response body
        /// </summary>
        /// <param name="context">The <see cref="IHttpListenerContext" /></param>
        /// <param name="propname">The boolean defining the Propfind propname request</param>
        /// <param name="responseDoc"></param>
        /// <returns>
        /// The <see cref="XmlDocument" /> containing the response body
        /// </returns>
        private XmlDocument ResponseDocument(IHttpListenerContext context, bool propname, XmlDocument responseDoc)
        {
            // Create the basic response XmlDocument
            const string responseXml = "<?xml version=\"1.0\"?><D:multistatus xmlns:D=\"DAV:\"></D:multistatus>";
            responseDoc.LoadXml(responseXml);

            // Generate the manager
            XmlNamespaceManager manager = new XmlNamespaceManager(responseDoc.NameTable);
            manager.AddNamespace("D", "DAV:");
            manager.AddNamespace("Office", "schemas-microsoft-com:office:office");
            manager.AddNamespace("Repl", "http://schemas.microsoft.com/repl/");
            manager.AddNamespace("Z", "urn:schemas-microsoft-com:");

            int count = 0;

            foreach (IWebDavStoreItem webDavStoreItem in _webDavStoreItems)
            {
                // Create the response element
                var allCustomProperties = webDavStoreItem.GetCustomProperties();

                WebDavProperty responseProperty = new WebDavProperty("response", string.Empty);
                XmlElement responseElement = responseProperty.ToXmlElement(responseDoc);
                //XmlElement responseElement = responseDoc.CreateElement(.....);

                // The href element
                Uri result;
                if (count == 0)
                {
                    Uri.TryCreate(_requestUri, string.Empty, out result);
                }
                else
                {
                    Uri.TryCreate(_requestUri, webDavStoreItem.Name, out result);
                }
                WebDavProperty hrefProperty = new WebDavProperty("href", result.AbsoluteUri);
                responseElement.AppendChild(hrefProperty.ToXmlElement(responseDoc));
                count++;

                // The propstat element
                WebDavProperty propstatProperty = new WebDavProperty("propstat", string.Empty);
                XmlElement propstatElement = propstatProperty.ToXmlElement(responseDoc);

                // The prop element
                WebDavProperty propProperty = new WebDavProperty("prop", string.Empty);
                XmlElement propElement = propProperty.ToXmlElement(responseDoc);

                //All properties but lockdiscovery and supportedlock can be handled here.
                foreach (WebDavProperty davProperty in _requestedProperties.Where(d => d.Name != "lockdiscovery" && d.Name != "supportedlock"))
                    propElement.AppendChild(PropChildElement(davProperty, responseDoc, webDavStoreItem, propname));

                //Custom property of the object.
                if (allCustomProperties != null)
                {
                    foreach (var setOfProperties in allCustomProperties)
                    {
                        manager.AddNamespace(setOfProperties.NamespacePrefix, setOfProperties.Namespace);
                        foreach (var property in setOfProperties.Properties)
                        {
                            propElement.AppendChild(property.ToXmlElement(manager, responseDoc));
                        }
                    }
                }

                //Since lockdiscovery returns an xml tree, we need to process it seperately.
                if (_requestedProperties.FirstOrDefault(d => d.Name == "lockdiscovery") != null)
                    propElement.AppendChild(LockDiscovery(result, ref responseDoc));

                //Since supportedlock returns an xml tree, we need to process it seperately.
                if (_requestedProperties.FirstOrDefault(d => d.Name == "supportedlock") != null)
                    propElement.AppendChild(SupportedLocks(ref responseDoc));

                // Add the prop element to the propstat element
                propstatElement.AppendChild(propElement);

                // The status element
                WebDavProperty statusProperty = new WebDavProperty("status",
                    "HTTP/1.1 " + context.Response.StatusCode + " " +
                    HttpWorkerRequest.GetStatusDescription(context.Response.StatusCode));
                propstatElement.AppendChild(statusProperty.ToXmlElement(responseDoc));

                // Add the propstat element to the response element
                responseElement.AppendChild(propstatElement);

                // Add the response element to the multistatus element
                responseDoc.DocumentElement.AppendChild(responseElement);
            }

            return responseDoc;
        }

        /// <summary>
        /// Gives the 
        /// <see cref="XmlElement" /> of a 
        /// <see cref="WebDavProperty" />
        /// with or without values
        /// or with or without child elements
        /// </summary>
        /// <param name="webDavProperty">The <see cref="WebDavProperty" /></param>
        /// <param name="xmlDocument">The <see cref="XmlDocument" /> containing the response body</param>
        /// <param name="iWebDavStoreItem">The <see cref="IWebDavStoreItem" /></param>
        /// <param name="isPropname">The boolean defining the Propfind propname request</param>
        /// <returns>
        /// The <see cref="XmlElement" /> of the <see cref="WebDavProperty" /> containing a value or child elements
        /// </returns>
        private static XmlElement PropChildElement(WebDavProperty webDavProperty, XmlDocument xmlDocument, IWebDavStoreItem iWebDavStoreItem, bool isPropname)
        {
            // If Propfind request contains a propname element
            if (isPropname)
            {
                webDavProperty.Value = String.Empty;
                return webDavProperty.ToXmlElement(xmlDocument);
            }

            // If not, add the values to webDavProperty
            webDavProperty.Value = GetWebDavPropertyValue(iWebDavStoreItem, webDavProperty);
            XmlElement xmlElement = webDavProperty.ToXmlElement(xmlDocument);


                // If the webDavProperty is the resourcetype property
                // and the webDavStoreItem is a collection
                // add the collection XmlElement as a child to the xmlElement
            if (webDavProperty.Name != "resourcetype" || !iWebDavStoreItem.IsCollection)
                return xmlElement;

            WebDavProperty collectionProperty = new WebDavProperty("collection", String.Empty);
            xmlElement.AppendChild(collectionProperty.ToXmlElement(xmlDocument));
            return xmlElement;
        }

        /// <summary>
        /// Gets the correct value for a <see cref="WebDavProperty" />
        /// </summary>
        /// <param name="webDavStoreItem">The <see cref="IWebDavStoreItem" /> defines the values</param>
        /// <param name="davProperty">The <see cref="WebDavProperty" /> that needs a value</param>
        /// <returns>
        /// A <see cref="string" /> containing the value
        /// </returns>
        private static string GetWebDavPropertyValue(IWebDavStoreItem webDavStoreItem, WebDavProperty davProperty)
        {
            switch (davProperty.Name)
            {
                case "creationdate":
                    return webDavStoreItem.CreationDate.ToUniversalTime().ToString("s") + "Z";
                case "displayname":
                    return webDavStoreItem.Name;
                case "getcontentlanguage":
                    //todo getcontentlanguage
                    return String.Empty;
                case "getcontentlength":
                    return (!webDavStoreItem.IsCollection ? "" + ((IWebDavStoreDocument)webDavStoreItem).Size : string.Empty);
                case "getcontenttype":
                    return (!webDavStoreItem.IsCollection ? "" + ((IWebDavStoreDocument)webDavStoreItem).MimeType : string.Empty);
                case "getetag":
                    return (!webDavStoreItem.IsCollection ? "" + ((IWebDavStoreDocument)webDavStoreItem).Etag : string.Empty);
                case "getlastmodified":
                    return webDavStoreItem.ModificationDate.ToUniversalTime().ToString("R");
                case "resourcetype":
                    //todo Add resourceType
                    return "";
                case "ishidden":
                    return webDavStoreItem.Hidden.ToString(CultureInfo.InvariantCulture);
                default:
                    return webDavStoreItem.GetProperty(davProperty);
            }
        }

        /// <summary>
        /// Returns an XML Fragment which details the supported locks on this implementation.
        /// 15.10 supportedlock Property
        /// Name:
        ///     supportedlock
        /// Purpose:
        ///     To provide a listing of the lock capabilities supported by the resource.
        /// Protected:
        ///     MUST be protected. Servers, not clients, determine what lock mechanisms are supported.
        /// COPY/MOVE behavior:
        ///    This property value is dependent on the kind of locks supported at the destination, not on the value of the property at the source resource. Servers attempting to COPY to a destination should not attempt to set this property at the destination.
        /// Description:
        ///     Returns a listing of the combinations of scope and access types that may be specified in a lock request on the resource. Note that the actual contents are themselves controlled by access controls, so a server is not required to provide information the client is not authorized to see. This property is NOT lockable with respect to write locks (Section 7).
        /// </summary>
        /// <param name="responsedoc"></param>
        /// <returns></returns>
        private static XmlNode SupportedLocks(ref XmlDocument responsedoc)
        {
            XmlNode node = new WebDavProperty("supportedlock").ToXmlElement(responsedoc);

            XmlNode lockentry = new WebDavProperty("lockentry").ToXmlElement(responsedoc);
            node.AppendChild(lockentry);

            XmlNode lockscope = new WebDavProperty("lockscope").ToXmlElement(responsedoc);
            lockentry.AppendChild(lockscope);

            XmlNode exclusive = new WebDavProperty("exclusive").ToXmlElement(responsedoc);
            lockscope.AppendChild(exclusive);

            XmlNode locktype = new WebDavProperty("locktype").ToXmlElement(responsedoc);
            lockentry.AppendChild(locktype);

            XmlNode write = new WebDavProperty("write").ToXmlElement(responsedoc);
            locktype.AppendChild(write);

            XmlNode lockentry1 = new WebDavProperty("lockentry").ToXmlElement(responsedoc);
            node.AppendChild(lockentry1);

            XmlNode lockscope1 = new WebDavProperty("lockscope").ToXmlElement(responsedoc);
            lockentry1.AppendChild(lockscope1);

            XmlNode shared = new WebDavProperty("shared").ToXmlElement(responsedoc);
            lockscope1.AppendChild(shared);

            XmlNode locktype1 = new WebDavProperty("locktype").ToXmlElement(responsedoc);
            lockentry1.AppendChild(locktype1);

            XmlNode write1 = new WebDavProperty("write").ToXmlElement(responsedoc);
            locktype1.AppendChild(write1);

            return node;
        }

        /// <summary>
        /// Returns the XML Format according to RFC
        /// Name:
        ///    lockdiscovery
        /// Purpose:
        ///     Describes the active locks on a resource
        /// Protected:
        ///     MUST be protected. Clients change the list of locks through LOCK and UNLOCK, not through PROPPATCH.
        /// COPY/MOVE behavior:
        ///     The value of this property depends on the lock state of the destination, not on the locks of the source resource. Recall 
        ///     that locks are not moved in a MOVE operation.
        /// Description:
        ///     Returns a listing of who has a lock, what type of lock he has, the timeout type and the time remaining on the timeout, 
        ///     and the associated lock token. Owner information MAY be omitted if it is considered sensitive. If there are no locks, but 
        ///     the server supports locks, the property will be present but contain zero 'activelock' elements. If there are one or more locks,
        ///     an 'activelock' element appears for each lock on the resource. This property is NOT lockable with respect to write locks (Section 7).
        /// </summary>
        /// <param name="path"></param>
        /// <param name="responsedoc"></param>
        /// <returns></returns>
        private static XmlNode LockDiscovery(Uri path, ref XmlDocument responsedoc)
        {
            XmlNode node = new WebDavProperty("lockdiscovery").ToXmlElement(responsedoc);
            foreach (var ilock in WebDavStoreItemLock.GetLocks(path))
            {
                XmlNode activelock = new WebDavProperty("activelock").ToXmlElement(responsedoc);
                node.AppendChild(activelock);

                XmlNode locktype = new WebDavProperty("locktype").ToXmlElement(responsedoc);
                activelock.AppendChild(locktype);

                XmlNode locktypeitem = new WebDavProperty(ilock.LockType.ToString().ToLower()).ToXmlElement(responsedoc);
                locktype.AppendChild(locktypeitem);

                XmlNode lockscope = new WebDavProperty("lockscope").ToXmlElement(responsedoc);
                activelock.AppendChild(lockscope);

                XmlNode lockscopeitem = new WebDavProperty(ilock.LockScope.ToString().ToLower()).ToXmlElement(responsedoc);
                lockscope.AppendChild(lockscopeitem);

                XmlNode depth = new WebDavProperty("depth").ToXmlElement(responsedoc);
                depth.InnerText = ilock.Depth.ToString(CultureInfo.InvariantCulture);
                activelock.AppendChild(depth);

                XmlNode owner = new WebDavProperty("owner").ToXmlElement(responsedoc);
                owner.InnerText = ilock.Owner;
                activelock.AppendChild(owner);

                XmlNode timeout = new WebDavProperty("timeout").ToXmlElement(responsedoc);
                timeout.InnerText = ilock.RequestedTimeout;
                activelock.AppendChild(timeout);

                XmlNode locktoken = new WebDavProperty("locktoken").ToXmlElement(responsedoc);
                activelock.AppendChild(locktoken);

                XmlNode tokenhref = new WebDavProperty("href").ToXmlElement(responsedoc);
                tokenhref.InnerText = ilock.Token;
                locktoken.AppendChild(tokenhref);

                XmlNode lockroot = new WebDavProperty("lockroot").ToXmlElement(responsedoc);
                activelock.AppendChild(lockroot);

                XmlNode lockroothref = new WebDavProperty("href").ToXmlElement(responsedoc);
                lockroothref.InnerText = ilock.Path.ToString();
                lockroot.AppendChild(lockroothref);
            }

            return node;
        }

        #endregion

        #region SendResponse

        /// <summary>
        /// Sends the response
        /// </summary>
        /// <param name="context">The <see cref="IHttpListenerContext" /> containing the response</param>
        /// <param name="responseDocument">The <see cref="XmlDocument" /> containing the response body</param>
        private static void SendResponse(IHttpListenerContext context, XmlNode responseDocument)
        {
            // convert the XmlDocument
            byte[] responseBytes = Encoding.UTF8.GetBytes(responseDocument.InnerXml);

            // HttpStatusCode doesn't contain WebDav status codes, but HttpWorkerRequest can handle these WebDav status codes
            context.Response.StatusCode = (int)WebDavStatusCode.MultiStatus;
            context.Response.StatusDescription = HttpWorkerRequest.GetStatusDescription((int)WebDavStatusCode.MultiStatus);

            context.Response.ContentLength64 = responseBytes.Length;
            context.Response.AdaptedInstance.ContentType = "text/xml";
            context.Response.OutputStream.Write(responseBytes, 0, responseBytes.Length);

            context.Response.Close();
        }

        #endregion

        #endregion

    }
}
