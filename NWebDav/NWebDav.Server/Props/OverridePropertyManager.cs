using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;

using NWebDav.Server.Http;
using NWebDav.Server.Stores;

namespace NWebDav.Server.Props
{
    public class OverridePropertyManager<TEntry> : IPropertyManager
        where TEntry : IStoreItem
    {
        private readonly Func<TEntry, IStoreItem> _converter;
        private readonly IDictionary<XName, DavProperty<TEntry>> _properties;
        private readonly IPropertyManager _basePropertyManager;

        public OverridePropertyManager(IEnumerable<DavProperty<TEntry>> properties, IPropertyManager basePropertyManager, Func<TEntry, IStoreItem> converter = null)
        {
            // Convert the properties to a dictionary for fast retrieval
            _properties = properties?.ToDictionary(p => p.Name) ?? throw new ArgumentNullException(nameof(properties));
            _basePropertyManager = basePropertyManager ?? throw new ArgumentNullException(nameof(basePropertyManager));
            _converter = converter ?? (si => si);

            // Create the property information immediately
            Properties = GetPropertyInfo();
        }

        public IList<PropertyInfo> Properties { get; }

        public bool HasProperty(XName name)
        {
            return _properties.ContainsKey(name);
        }

        public async ValueTask<object> GetPropertyAsync(IHttpContext httpContext, IStoreItem item, XName propertyName, bool skipExpensive = false)
        {
            // Find the property
            if (!_properties.TryGetValue(propertyName, out var property))
                return await _basePropertyManager.GetPropertyAsync(httpContext, _converter((TEntry)item), propertyName, skipExpensive);

            // Check if the property has a getter
            if (property.GetterAsync == null)
                return await _basePropertyManager.GetPropertyAsync(httpContext, _converter((TEntry)item), propertyName, skipExpensive);

            // Skip expensive properties
            if (skipExpensive && property.IsExpensive)
                return null;

            // Obtain the value
            return await property.GetterAsync(httpContext, (TEntry)item);
        }

        public async ValueTask<(bool IsExists, object Value)> TryGetPropertyAsync(IHttpContext httpContext, IStoreItem item, XName propertyName, bool skipExpensive = false)
        {
            // Find the property
            if (!_properties.TryGetValue(propertyName, out var property))
                return await _basePropertyManager.TryGetPropertyAsync(httpContext, _converter((TEntry)item), propertyName, skipExpensive);

            // Check if the property has a getter
            if (property.GetterAsync == null)
                return await _basePropertyManager.TryGetPropertyAsync(httpContext, _converter((TEntry)item), propertyName, skipExpensive);

            // Skip expensive properties
            if (skipExpensive && property.IsExpensive)
                return (true, null);

            // Obtain the value
            return (true, await property.GetterAsync(httpContext, (TEntry)item));
        }

        public async Task<DavStatusCode> SetPropertyAsync(IHttpContext httpContext, IStoreItem item, XName propertyName, object value)
        {
            // Find the property
            if (!_properties.TryGetValue(propertyName, out var property))
                return await _basePropertyManager.SetPropertyAsync(httpContext, _converter((TEntry)item), propertyName, value);

            // Check if the property has a setter
            if (property.SetterAsync == null)
                return await _basePropertyManager.SetPropertyAsync(httpContext, _converter((TEntry)item), propertyName, value);

            // Set the value
            return await property.SetterAsync(httpContext, (TEntry)item, value);
        }

        private IList<PropertyInfo> GetPropertyInfo()
        {
            // Obtain the base properties that do not have an override
            var basePropertyInfo = _basePropertyManager.Properties.Where(p => !_properties.ContainsKey(p.Name));
            var overridePropertyInfo = _properties.Values.Where(p => p.GetterAsync != null || p.SetterAsync != null).Select(p => new PropertyInfo(p.Name, p.IsExpensive));

            // Combine both lists
            return basePropertyInfo.Concat(overridePropertyInfo).ToList();
        }
    }
}