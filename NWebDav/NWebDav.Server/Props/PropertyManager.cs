using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;

using NWebDav.Server.Http;
using NWebDav.Server.Stores;

namespace NWebDav.Server.Props
{
    /// <summary>
    /// Property manager that handles all the properties for a specific store
    /// item and collection. 
    /// </summary>
    /// <remarks>
    /// The default property manager is used to define the root property
    /// manager for a store.
    /// </remarks>
    /// <typeparam name="TEntry">
    /// Store item or collection to which this DAV property applies.
    /// </typeparam>
    public class PropertyManager<TEntry> : IPropertyManager where TEntry : IStoreItem
    {
        private readonly IDictionary<XName, DavProperty<TEntry>> _properties;

        /// <summary>
        /// Create an instance of the default property manager implementation.
        /// </summary>
        /// <param name="properties">
        /// Set of WebDAV properties that are implemented by the property
        /// manager for the store item/collection type.
        /// </param>
        public PropertyManager(IEnumerable<DavProperty<TEntry>> properties)
        {
            // If properties are supported, then the properties should be set
            if (properties == null)
                throw new ArgumentNullException(nameof(properties));

            // Convert the properties to a dictionary for fast retrieval
            _properties = properties.ToDictionary(p => p.Name);

            // Create the property information immediately
            Properties = _properties.Select(p => new PropertyInfo(p.Value.Name, p.Value.IsExpensive)).ToList();
        }

        /// <summary>
        /// Obtain the list of all implemented properties.
        /// </summary>
        public IList<PropertyInfo> Properties { get; }

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
        public Task<object> GetPropertyAsync(IHttpContext httpContext, IStoreItem item, XName propertyName, bool skipExpensive = false)
        {
            // Find the property
            if (!_properties.TryGetValue(propertyName, out var property))
                return Task.FromResult((object)null);

            // Check if the property has a getter
            if (property.GetterAsync == null)
                return Task.FromResult((object)null);

            // Skip expensive properties
            if (skipExpensive && property.IsExpensive)
                return Task.FromResult((object)null);

            // Obtain the value
            return property.GetterAsync(httpContext, (TEntry)item);
        }

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
        public Task<DavStatusCode> SetPropertyAsync(IHttpContext httpContext, IStoreItem item, XName propertyName, object value)
        {
            // Find the property
            if (!_properties.TryGetValue(propertyName, out var property))
                return Task.FromResult(DavStatusCode.NotFound);

            // Check if the property has a setter
            if (property.SetterAsync == null)
                return Task.FromResult(DavStatusCode.Conflict);

            // Set the value
            return property.SetterAsync(httpContext, (TEntry)item, value);
        }
    }
}
