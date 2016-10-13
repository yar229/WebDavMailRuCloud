using System;
using System.Collections.Generic;
using System.Xml.Linq;

using NWebDav.Server.Http;
using NWebDav.Server.Stores;

namespace NWebDav.Server.Props
{
    public interface IPropertyManager
    {
        /// <summary>
        /// Obtain the list of all implemented properties.
        /// </summary>
        IList<PropertyInfo> Properties { get; }
        object GetProperty(IHttpContext httpContext, IStoreItem item, XName name, bool skipExpensive = false);
        DavStatusCode SetProperty(IHttpContext httpContext, IStoreItem item, XName name, object value);
    }
}
