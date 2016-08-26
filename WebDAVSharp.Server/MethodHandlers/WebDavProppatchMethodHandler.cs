using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Xml;

using WebDAVSharp.Server.Adapters;
using WebDAVSharp.Server.Stores;
using WebDAVSharp.Server.Utilities;

using System.Xml.Linq;

namespace WebDAVSharp.Server.MethodHandlers
{
    /// <summary>
    /// This class implements the <c>PROPPATCH</c> HTTP method for WebDAV#.
    /// </summary>
    internal class WebDavProppatchMethodHandler : WebDavMethodHandlerBase
    {

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
                    "PROPPATCH"
                };
            }
        }

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
        /// <param name="response"></param>
        /// <param name="request"></param>
        protected override void OnProcessRequest(
           WebDavServer server,
           IHttpListenerContext context,
           IWebDavStore store,
           XmlDocument request,
           XmlDocument response)
        {
            /***************************************************************************************************
             * Retreive al the information from the request
             ***************************************************************************************************/

            // Get the URI to the location
            Uri requestUri = context.Request.Url;

            // Initiate the XmlNamespaceManager and the XmlNodes
            XmlNamespaceManager manager = null;
            XmlNode propNode = null;
            XDocument requestXDocument = null;
            // try to read the body
            try
            {
                StreamReader reader = new StreamReader(context.Request.InputStream, Encoding.UTF8);
                string requestBody = reader.ReadToEnd();

                if (!String.IsNullOrEmpty(requestBody))
                {
                    request.LoadXml(requestBody);

                    if (request.DocumentElement != null)
                    {
                        if (request.DocumentElement.LocalName != "propertyupdate")
                        {
                            WebDavServer.Log.Debug("PROPPATCH method without propertyupdate element in xml document");
                        }

                        manager = new XmlNamespaceManager(request.NameTable);
                        manager.AddNamespace("D", "DAV:");
                        manager.AddNamespace("Office", "schemas-microsoft-com:office:office");
                        manager.AddNamespace("Repl", "http://schemas.microsoft.com/repl/");
                        manager.AddNamespace("Z", "urn:schemas-microsoft-com:");

                        propNode = request.DocumentElement.SelectSingleNode("D:set/D:prop", manager);
                    }

                    requestXDocument = XDocument.Parse(requestBody);
                }
            }
            catch (Exception ex)
            {
                WebDavServer.Log.Warn(ex.Message);
            }

            /***************************************************************************************************
             * Take action
             ***************************************************************************************************/

            // Get the parent collection of the item
            IWebDavStoreCollection collection = GetParentCollection(server, store, context.Request.Url);

            // Get the item from the collection
            IWebDavStoreItem item = GetItemFromCollection(collection, context.Request.Url);

            //we need to get properties to set 
            List<WebDavProperty> propertiesToSet = new List<WebDavProperty>();
            if (requestXDocument != null)
            {
                foreach (XElement propertySet in requestXDocument.Descendants()
                    .Where(n => n.Name.LocalName == "set"))
                {
                    //this is a property to set
                    XElement propSetNode = propertySet.Elements().First().Elements().First();
                    propertiesToSet.Add (new WebDavProperty() {
                        Name = propSetNode.Name.LocalName,
                        Namespace = propSetNode.Name.NamespaceName,
                        Value = propSetNode.Value
                    });
                }
            }
            item.SetProperties(propertiesToSet);
          
            /***************************************************************************************************
             * Create the body for the response
             ***************************************************************************************************/

            // Create the basic response XmlDocument
            const string responseXml = "<?xml version=\"1.0\" encoding=\"utf-8\" ?><D:multistatus " +
                                       "xmlns:Z=\"urn:schemas-microsoft-com:\" xmlns:D=\"DAV:\">" +
                                       "<D:response></D:response></D:multistatus>";
            response.LoadXml(responseXml);

            // Select the response node
            XmlNode responseNode = response.DocumentElement.SelectSingleNode("D:response", manager);

            // Add the elements

            // The href element
            WebDavProperty hrefProperty = new WebDavProperty("href", requestUri.ToString());
            responseNode.AppendChild(hrefProperty.ToXmlElement(response));

            // The propstat element
            WebDavProperty propstatProperty = new WebDavProperty("propstat", string.Empty);
            XmlElement propstatElement = propstatProperty.ToXmlElement(response);

            // The propstat/status element
            WebDavProperty statusProperty = new WebDavProperty("status", "HTTP/1.1 " + context.Response.StatusCode + " " +
                    HttpWorkerRequest.GetStatusDescription(context.Response.StatusCode));
            propstatElement.AppendChild(statusProperty.ToXmlElement(response));

            // The other propstat children
            foreach (WebDavProperty property in from XmlNode child in propNode.ChildNodes
                where child.Name.ToLower()
                    .Contains("creationtime") || child.Name.ToLower()
                        .Contains("fileattributes") || child.Name.ToLower()
                            .Contains("lastaccesstime") || child.Name.ToLower()
                                .Contains("lastmodifiedtime")
                let node = propNode.SelectSingleNode(child.Name, manager)

                select new WebDavProperty(child.LocalName, string.Empty, node != null ? node.NamespaceURI : string.Empty))

                propstatElement.AppendChild(property.ToXmlElement(response));

            responseNode.AppendChild(propstatElement);

            /***************************************************************************************************
            * Send the response
            ***************************************************************************************************/
            
            // convert the StringBuilder
            string resp = response.InnerXml;
            byte[] responseBytes = Encoding.UTF8.GetBytes(resp);


            // HttpStatusCode doesn't contain WebDav status codes, but HttpWorkerRequest can handle these WebDav status codes
            context.Response.StatusCode = (int)WebDavStatusCode.MultiStatus;
            context.Response.StatusDescription = HttpWorkerRequest.GetStatusDescription((int)WebDavStatusCode.MultiStatus);

            // set the headers of the response
            context.Response.ContentLength64 = responseBytes.Length;
            context.Response.AdaptedInstance.ContentType = "text/xml";

            // the body
            context.Response.OutputStream.Write(responseBytes, 0, responseBytes.Length);

            context.Response.Close();
        }

        #endregion

    }
}