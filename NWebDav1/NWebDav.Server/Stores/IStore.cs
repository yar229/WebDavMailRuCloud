using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

using NWebDav.Server.Http;
using NWebDav.Server.Locking;
using NWebDav.Server.Props;

namespace NWebDav.Server.Stores
{
    public struct StoreItemResult
    {
        public DavStatusCode Result { get; }
        public IStoreItem Item { get; }

        public StoreItemResult(DavStatusCode result, IStoreItem item = null)
        {
            Result = result;
            Item = item;
        }

        public static bool operator!=(StoreItemResult left, StoreItemResult right)
        {
            return !(left == right);
        }

        public static bool operator==(StoreItemResult left, StoreItemResult right)
        {
            return left.Result == right.Result && (left.Item == null && right.Item == null || left.Item != null && left.Item.Equals(right.Item));
        }

        public override bool Equals(object obj)
        {
            if (!(obj is StoreItemResult))
                return false;
            return this == (StoreItemResult)obj;
        }

        public override int GetHashCode() => Result.GetHashCode() ^ (Item?.GetHashCode() ?? 0);
    }

    public struct StoreCollectionResult
    {
        public DavStatusCode Result { get; }
        public IStoreCollection Collection { get; }

        public StoreCollectionResult(DavStatusCode result, IStoreCollection collection = null)
        {
            Result = result;
            Collection = collection;
        }

        public static bool operator !=(StoreCollectionResult left, StoreCollectionResult right)
        {
            return !(left == right);
        }

        public static bool operator ==(StoreCollectionResult left, StoreCollectionResult right)
        {
            return left.Result == right.Result && (left.Collection == null && right.Collection == null || left.Collection != null && left.Collection.Equals(right.Collection));
        }

        public override bool Equals(object obj)
        {
            if (!(obj is StoreCollectionResult))
                return false;
            return this == (StoreCollectionResult)obj;
        }

        public override int GetHashCode() => Result.GetHashCode() ^ (Collection?.GetHashCode() ?? 0);
    }

    public interface IStore
    {
        Task<IStoreItem> GetItemAsync(Uri uri, IHttpContext httpContext);
        Task<IStoreCollection> GetCollectionAsync(Uri uri, IHttpContext httpContext);
    }

    public interface IStoreItem
    {
        // Item properties
        string Name { get; }
        string UniqueKey { get; }

        // Read/Write access to the data
        Task<Stream> GetReadableStreamAsync(IHttpContext httpContext);
        Task<DavStatusCode> UploadFromStreamAsync(IHttpContext httpContext, Stream source);

        // Copy support
        Task<StoreItemResult> CopyAsync(IStoreCollection destination, string name, bool overwrite, IHttpContext httpContext);

        // Property support
        IPropertyManager PropertyManager { get; }

        // Locking support
        ILockingManager LockingManager { get; }
    }

    public interface IStoreCollection : IStoreItem
    {
        // Get specific item (or all items)
        Task<IStoreItem> GetItemAsync(string name, IHttpContext httpContext);
        Task<IList<IStoreItem>> GetItemsAsync(IHttpContext httpContext);

        // Create items and collections and add to the collection
        Task<StoreItemResult> CreateItemAsync(string name, bool overwrite, IHttpContext httpContext);
        Task<StoreCollectionResult> CreateCollectionAsync(string name, bool overwrite, IHttpContext httpContext);

        // Move items between collections
        Task<StoreItemResult> MoveItemAsync(string sourceName, IStoreCollection destination, string destinationName, bool overwrite, IHttpContext httpContext);

        // Delete items from collection
        Task<DavStatusCode> DeleteItemAsync(string name, IHttpContext httpContext);

        InfiniteDepthMode InfiniteDepthMode { get; }
    }

    /// <summary>
    /// When the Depth is set to infinite, then this enumeration specifies
    /// how to deal with this.
    /// </summary>
    public enum InfiniteDepthMode
    {
        /// <summary>
        /// Infinite depth is allowed (this is according spec).
        /// </summary>
        Allowed,

        /// <summary>
        /// Infinite depth is not allowed (this results in HTTP 403 Forbidden).
        /// </summary>
        Rejected,

        /// <summary>
        /// Infinite depth is handled as Depth 0.
        /// </summary>
        Assume0,

        /// <summary>
        /// Infinite depth is handled as Depth 1.
        /// </summary>
        Assume1
    }
}
