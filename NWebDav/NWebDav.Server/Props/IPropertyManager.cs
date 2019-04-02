using System.Collections.Generic;
using System.Threading.Tasks;
using System.Xml.Linq;

using NWebDav.Server.Http;
using NWebDav.Server.Stores;

namespace NWebDav.Server.Props
{
    /// <summary>
    /// This interface defines the property manager that is responsible to
    /// handle all the properties for store items and collections.
    /// </summary>
    public interface IPropertyManager
    {
        /// <summary>
        /// Obtain the list of all implemented properties.
        /// </summary>
        IList<PropertyInfo> Properties { get; }

        /// <summary>
        /// Get the value of the specified property for the given item.
        /// </summary>
        /// <param name="httpContext">
        /// HTTP context of the current request.
        /// </param>
        /// <param name="item">
        /// Store item/collection for which the property should be obtained.
        /// </param>
        /// <param name="propertyName">
        /// Name of the property (including namespace).
        /// </param>
        /// <param name="skipExpensive">
        /// Flag indicating whether to skip the property if it is too expensive
        /// to compute.
        /// </param>
        /// <returns>
        /// A task that represents the get property operation. The task will
        /// return the property value or <see langword="null"/> if
        /// <paramref name="skipExpensive"/> is set to <see langword="true"/>
        /// and the parameter is expensive to compute.
        /// </returns>
        Task<object> GetPropertyAsync(IHttpContext httpContext, IStoreItem item, XName propertyName, bool skipExpensive = false);

        /// <summary>
        /// Set the value of the specified property for the given item.
        /// </summary>
        /// <param name="httpContext">
        /// HTTP context of the current request.
        /// </param>
        /// <param name="item">
        /// Store item/collection for which the property should be obtained.
        /// </param>
        /// <param name="propertyName">
        /// Name of the property (including namespace).
        /// </param>
        /// <param name="value">
        /// New value of the property.
        /// </param>
        /// <returns>
        /// A task that represents the set property operation. The task will
        /// return the WebDAV status code of the set operation upon completion.
        /// </returns>
        Task<DavStatusCode> SetPropertyAsync(IHttpContext httpContext, IStoreItem item, XName propertyName, object value);
    }
}
