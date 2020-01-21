using System;
using System.Net;
using System.Threading.Tasks;
using NWebDav.Server;
using NWebDav.Server.Http;
using NWebDav.Server.Locking;
using NWebDav.Server.Stores;
using YaR.Clouds.Base;

namespace YaR.Clouds.WebDavStore.StoreBase
{
    public sealed class LocalStore : IStore
    {
        public LocalStore(bool isWritable = true, ILockingManager lockingManager = null)
        {
            LockingManager = lockingManager ?? new InMemoryLockingManager();
            IsWritable = isWritable;
        }

        private bool IsWritable { get; }
        private ILockingManager LockingManager { get; }

        public async Task<IStoreItem> GetItemAsync(WebDavUri uri, IHttpContext httpContext)
        {
            var identity = (HttpListenerBasicIdentity)httpContext.Session.Principal.Identity;
            var path = uri.Path;
            
            try
            {
                var item = await CloudManager.Instance(identity).GetItemAsync(path);
                if (item != null)
                {
                    return item.IsFile
                        ? new LocalStoreItem(LockingManager, (File)item, IsWritable)
                        : new LocalStoreCollection(httpContext, LockingManager, (Folder)item, IsWritable) as IStoreItem;
                }
            }
            // ReSharper disable once RedundantCatchClause
            #pragma warning disable 168
            catch (Exception ex)
            #pragma warning restore 168
            {
                throw;
            }

            return null;
        }

        public async Task<IStoreCollection> GetCollectionAsync(WebDavUri uri, IHttpContext httpContext)
        {
            var path = uri.Path;

            var item = await CloudManager.Instance(httpContext.Session.Principal.Identity)
                .GetItemAsync(path, Cloud.ItemType.Folder);

            return new LocalStoreCollection(httpContext, LockingManager, (Folder)item, IsWritable);
        }
    }
}
