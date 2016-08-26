using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Principal;
using System.Text;
using System.Threading;
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
    /// This class implements the <c>LOCK</c> HTTP method for WebDAV#.
    /// </summary>
    internal class WebDavLockMethodHandler : WebDavMethodHandlerBase
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
                    "LOCK"
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
        /// <exception cref="WebDAVSharp.Server.Exceptions.WebDavPreconditionFailedException"></exception>
        /// <param name="response"></param>
        /// <param name="request"></param>
        protected override void OnProcessRequest(
          WebDavServer server,
          IHttpListenerContext context,
          IWebDavStore store,
          XmlDocument request,
          XmlDocument response)
        {

            if (!WebDavStoreItemLock.LockEnabled) throw new WebDavNotImplementedException("Lock support disabled");
            /***************************************************************************************************
             * Retreive al the information from the request
             ***************************************************************************************************/

            // read the headers
            int depth = GetDepthHeader(context.Request);
            string timeout = GetTimeoutHeader(context.Request);
            string locktoken = GetLockTokenIfHeader(context.Request);
            int lockResult = 0;
            // Initiate the XmlNamespaceManager and the XmlNodes
            XmlNamespaceManager manager;
            XmlNode lockscopeNode, locktypeNode, ownerNode;

            if (string.IsNullOrEmpty(locktoken))
            {
                #region New Lock

                // try to read the body
                try
                {
                    StreamReader reader = new StreamReader(context.Request.InputStream, Encoding.UTF8);
                    string requestBody = reader.ReadToEnd();

                    if (!requestBody.Equals("") && requestBody.Length != 0)
                    {

                        request.LoadXml(requestBody);

                        if (request.DocumentElement != null &&
                            request.DocumentElement.LocalName != "prop" &&
                            request.DocumentElement.LocalName != "lockinfo")
                        {
                            WebDavServer.Log.Debug("LOCK method without prop or lockinfo element in xml document");
                        }

                        manager = new XmlNamespaceManager(request.NameTable);
                        manager.AddNamespace("D", "DAV:");
                        manager.AddNamespace("Office", "schemas-microsoft-com:office:office");
                        manager.AddNamespace("Repl", "http://schemas.microsoft.com/repl/");
                        manager.AddNamespace("Z", "urn:schemas-microsoft-com:");

                        // Get the lockscope, locktype and owner as XmlNodes from the XML document
                        lockscopeNode = request.DocumentElement.SelectSingleNode("D:lockscope", manager);
                        locktypeNode = request.DocumentElement.SelectSingleNode("D:locktype", manager);
                        ownerNode = request.DocumentElement.SelectSingleNode("D:owner", manager);
                    }
                    else
                    {
                        throw new WebDavPreconditionFailedException();
                    }
                }
                catch (Exception ex)
                {
                    WebDavServer.Log.Warn(ex.Message);
                    throw;
                }


                /***************************************************************************************************
                 * Lock the file or folder
                 ***************************************************************************************************/


                // Get the parent collection of the item
                IWebDavStoreCollection collection = GetParentCollection(server, store, context.Request.Url);

                WebDavLockScope lockscope = (lockscopeNode.InnerXml.StartsWith("<D:exclusive"))
                    ? WebDavLockScope.Exclusive
                    : WebDavLockScope.Shared;

                //Only lock available at this time is a Write Lock according to RFC
                WebDavLockType locktype = (locktypeNode.InnerXml.StartsWith("<D:write"))
                    ? WebDavLockType.Write
                    : WebDavLockType.Write;

                string userAgent = context.Request.Headers["User-Agent"];

                WindowsIdentity Identity = (WindowsIdentity)Thread.GetData(Thread.GetNamedDataSlot(WebDavServer.HttpUser));

                // Get the item from the collection
                try
                {
                    var item = GetItemFromCollection(collection, context.Request.Url);
                    String lockLogicalKey = context.Request.Url.AbsoluteUri;
                    if (item != null)
                    {
                        lockLogicalKey = item.LockLogicalKey;
                    }
                    if (item != null && !item.Lock())
                    {
                        lockResult = 423; //Resource cannot be locked
                    }
                    else
                    {
                        //try acquiring a standard lock, 
                        lockResult = WebDavStoreItemLock.Lock(
                            context.Request.Url, 
                            lockLogicalKey,
                            lockscope, 
                            locktype, 
                            Identity.Name, 
                            userAgent,
                            ref timeout, 
                            out locktoken, 
                            request, 
                            depth);
                    }
                }
                catch (Exception ex)
                {
                    WebDavServer._log.Error(String.Format("Error occourred while acquiring lock {0}", context.Request.Url), ex);
                    lockResult = 423; //Resource cannot be locked some exception occurred
                }
                #endregion
            }
            else
            {
                #region Refreshing a lock
                //Refresh lock will ref us back the original XML document which was used to request this lock, from
                //this we will grab the data we need to build the response to the lock refresh request.
                lockResult = WebDavStoreItemLock.RefreshLock(context.Request.Url, locktoken, ref timeout, out request);
                if (request == null)
                {
                    context.SendSimpleResponse(409);
                    return;
                }

                try
                {
                    if (request.DocumentElement != null &&
                        request.DocumentElement.LocalName != "prop" &&
                        request.DocumentElement.LocalName != "lockinfo")
                    {
                        WebDavServer.Log.Debug("LOCK method without prop or lockinfo element in xml document");
                    }

                    manager = new XmlNamespaceManager(request.NameTable);
                    manager.AddNamespace("D", "DAV:");
                    manager.AddNamespace("Office", "schemas-microsoft-com:office:office");
                    manager.AddNamespace("Repl", "http://schemas.microsoft.com/repl/");
                    manager.AddNamespace("Z", "urn:schemas-microsoft-com:");

                    // Get the lockscope, locktype and owner as XmlNodes from the XML document
                    lockscopeNode = request.DocumentElement.SelectSingleNode("D:lockscope", manager);
                    locktypeNode = request.DocumentElement.SelectSingleNode("D:locktype", manager);
                    ownerNode = request.DocumentElement.SelectSingleNode("D:owner", manager);
                }
                catch (Exception ex)
                {
                    WebDavServer.Log.Warn(ex.Message);
                    throw;
                }

                #endregion
            }

            /***************************************************************************************************
             * Create the body for the response
             ***************************************************************************************************/

            // Create the basic response XmlDocument
            const string responseXml = "<?xml version=\"1.0\" encoding=\"utf-8\" ?><D:prop xmlns:D=\"DAV:\"><D:lockdiscovery><D:activelock/></D:lockdiscovery></D:prop>";
            response.LoadXml(responseXml);

            // Select the activelock XmlNode
            XmlNode activelock = response.DocumentElement.SelectSingleNode("D:lockdiscovery/D:activelock", manager);

            // Import the given nodes
            activelock.AppendChild(response.ImportNode(lockscopeNode, true));
            activelock.AppendChild(response.ImportNode(locktypeNode, true));
            activelock.AppendChild(response.ImportNode(ownerNode, true));

            // Add the additional elements, e.g. the header elements

            // The timeout element
            WebDavProperty timeoutProperty = new WebDavProperty("timeout", timeout);// timeout);
            activelock.AppendChild(timeoutProperty.ToXmlElement(response));

            // The depth element
            WebDavProperty depthProperty = new WebDavProperty("depth", (depth == 0 ? "0" : "Infinity"));
            activelock.AppendChild(depthProperty.ToXmlElement(response));

            // The locktoken element
            WebDavProperty locktokenProperty = new WebDavProperty("locktoken", string.Empty);
            XmlElement locktokenElement = locktokenProperty.ToXmlElement(response);
            WebDavProperty hrefProperty = new WebDavProperty("href", locktoken);//"opaquelocktoken:e71d4fae-5dec-22df-fea5-00a0c93bd5eb1");
            locktokenElement.AppendChild(hrefProperty.ToXmlElement(response));
            activelock.AppendChild(locktokenElement);

            // The lockroot element
            WebDavProperty lockRootProperty = new WebDavProperty("lockroot", string.Empty);
            XmlElement lockRootElement = lockRootProperty.ToXmlElement(response);
            WebDavProperty hrefRootProperty = new WebDavProperty("href", context.Request.Url.AbsoluteUri);//"lockroot
            lockRootElement.AppendChild(hrefRootProperty.ToXmlElement(response));
            activelock.AppendChild(lockRootElement);

            /***************************************************************************************************
             * Send the response
             ***************************************************************************************************/

            // convert the StringBuilder
            string resp = response.InnerXml;
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


            byte[] responseBytes = Encoding.UTF8.GetBytes(resp);


            context.Response.StatusCode = lockResult;
            context.Response.StatusDescription = HttpWorkerRequest.GetStatusDescription(lockResult);

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