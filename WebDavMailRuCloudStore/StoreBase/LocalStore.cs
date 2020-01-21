using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using System.Xml.Linq;
using NWebDav.Server;
using NWebDav.Server.Http;
using NWebDav.Server.Locking;
using NWebDav.Server.Stores;
using YaR.Clouds.Base;

namespace YaR.Clouds.WebDavStore.StoreBase
{
    //public class EmptyLockingManager : ILockingManager
    //{
    //    public LockResult Lock(IStoreItem item, LockType lockType, LockScope lockScope, XElement owner, WebDavUri lockRootUri,
    //        bool recursiveLock, IEnumerable<int> timeouts)
    //    {
    //        return LR;
    //    }
    //    static LockResult LR = new LockResult(DavStatusCode.Ok);

    //    public DavStatusCode Unlock(IStoreItem item, WebDavUri token)
    //    {
    //        return DavStatusCode.Ok;
    //    }

    //    public LockResult RefreshLock(IStoreItem item, bool recursiveLock, IEnumerable<int> timeouts, WebDavUri lockTokenUri)
    //    {
    //        return LR;
    //    }

    //    public IEnumerable<ActiveLock> GetActiveLockInfo(IStoreItem item)
    //    {
    //        yield break;
    //    }

    //    public IEnumerable<LockEntry> GetSupportedLocks(IStoreItem item)
    //    {
    //        yield break;
    //    }

    //    public bool IsLocked(IStoreItem item)
    //    {
    //        return false;
    //    }

    //    public bool HasLock(IStoreItem item, WebDavUri lockToken)
    //    {
    //        return false;
    //    }
    //}


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
