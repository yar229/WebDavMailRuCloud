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
        Stream GetReadableStream(IHttpContext httpContext);
        Stream GetWritableStream(IHttpContext httpContext);

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

        bool AllowInfiniteDepthProperties { get; }
    }
}
