using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Xml.Linq;

using NWebDav.Server.Http;
using NWebDav.Server.Stores;

namespace NWebDav.Server.Props
{
    /// <summary>
    /// Abstract base class representing a single DAV property.
    /// </summary>
    /// <remarks>
    /// Although it is possible to derive directly from this class, it is more
    /// convenient to derive from the typed classes.
    /// </remarks>
    /// <typeparam name="TEntry">
    /// Store item or collection to which this DAV property applies.
    /// </typeparam>
    [DebuggerDisplay("{Name}")]
    public abstract class DavProperty<TEntry> where TEntry : IStoreItem
    {
        /// <summary>
        /// Full name of the property
        /// </summary>
        /// <value>Full name of the property (including the namespace).</value>
        public abstract XName Name { get; }

        /// <summary>
        /// Gets or sets the delegate that is responsible to obtain the
        /// property's value of a store item/collection.
        /// </summary>
        /// <returns>Delegate to obtain the property value.</returns>
        /// <remarks>
        /// An <see cref="IHttpContext"/> object is passed to the getter
        /// delegate that contains the HTTP context. It is typically used
        /// to deal with compatibility of certain WebDAV clients (can be
        /// determined using the user agent).
        /// </remarks>
        public Func<IHttpContext, TEntry, Task<object>> GetterAsync { get; set; }

        /// <summary>
        /// Gets or sets the delegate that is responsible to set the property
        /// value of a store item/collection.
        /// </summary>
        /// <returns>Delegate to set the property value.</returns>
        public Func<IHttpContext, TEntry, object, Task<DavStatusCode>> SetterAsync { get; set; }

        /// <summary>
        /// Gets or sets the flag whether or not the property is expensive.
        /// </summary>
        /// <returns>
        /// <see langword="true"/> if the property is expensive to calculate or
        /// <see langword="false"/> (default) otherwise.
        /// </returns>
        /// <remarks>
        /// <para>
        /// A property should be considered expensive if it takes a
        /// considerable amount of CPU cycles to determine its value. It is
        /// also considered expensive if the calculation doesn't require a
        /// lot of CPU cycles, but takes a lot of time. Calculating a hash
        /// value of an item is considered expensive, because it requires to
        /// scan the entire file to determine the value.
        /// </para>
        /// <para>
        /// Expensive properties are not returned in the GET or HEAD headers
        /// to prevent excessive load on the server. They are also skipped
        /// when requesting all the properties of an item.
        /// </para>
        /// </remarks>
        public bool IsExpensive { get; set; }
    }
}
