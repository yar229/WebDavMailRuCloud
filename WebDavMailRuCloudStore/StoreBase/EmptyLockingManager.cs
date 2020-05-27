using System;
using System.Linq;
using System.Collections.Generic;
using System.Xml.Linq;
using NWebDav.Server;
using NWebDav.Server.Locking;
using NWebDav.Server.Stores;

namespace YaR.Clouds.WebDavStore.StoreBase
{
    public class EmptyLockingManager : ILockingManager
    {
        private class ItemLockInfo
        {
            public Guid Token { get; }
            public IStoreItem Item { get; }
            public LockType Type { get; }
            public LockScope Scope { get; }
            public WebDavUri LockRootUri { get; }
            public bool Recursive { get; }
            public XElement Owner { get; }
            public int Timeout { get; }
            public DateTime? Expires { get; private set; }
            public bool IsExpired => !Expires.HasValue || Expires < DateTime.UtcNow;

            public ItemLockInfo(IStoreItem item, LockType lockType, LockScope lockScope, WebDavUri lockRootUri, bool recursive, XElement owner, int timeout)
            {
                Token = Guid.NewGuid();
                Item = item;
                Type = lockType;
                Scope = lockScope;
                LockRootUri = lockRootUri;
                Recursive = recursive;
                Owner = owner;
                Timeout = timeout;

                RefreshExpiration(timeout);
            }

            public void RefreshExpiration(int timeout)
            {
                Expires = timeout >= 0 ? (DateTime?)DateTime.UtcNow.AddSeconds(timeout) : null;
            }
        }


        public LockResult Lock(IStoreItem item, LockType lockType, LockScope lockScope, XElement owner, WebDavUri lockRootUri,
            bool recursiveLock, IEnumerable<int> timeouts)
        {
            var timeout = timeouts.Cast<int?>().FirstOrDefault();
            var itemLockInfo = new ItemLockInfo(item, lockType, lockScope, lockRootUri, recursiveLock, owner, timeout ?? -1);
            return new LockResult(DavStatusCode.Ok, GetActiveLockInfo(itemLockInfo));
        }
        //static readonly LockResult LR = new LockResult(DavStatusCode.Ok, new ActiveLock());

        public DavStatusCode Unlock(IStoreItem item, WebDavUri token)
        {
            return DavStatusCode.Ok;
        }

        public LockResult RefreshLock(IStoreItem item, bool recursiveLock, IEnumerable<int> timeouts, WebDavUri lockTokenUri)
        {
            var timeout = timeouts.Cast<int?>().FirstOrDefault();
            var itemLockInfo = new ItemLockInfo(item, LockType.Write, LockScope.Exclusive, lockTokenUri, recursiveLock, null, timeout ?? -1);
            return new LockResult(DavStatusCode.Ok, GetActiveLockInfo(itemLockInfo));
        }

        private const string TokenScheme = "opaquelocktoken";

        private static ActiveLock GetActiveLockInfo(ItemLockInfo itemLockInfo)
        {
            return new ActiveLock(itemLockInfo.Type, itemLockInfo.Scope, itemLockInfo.Recursive ? int.MaxValue : 0, itemLockInfo.Owner, itemLockInfo.Timeout, new WebDavUri($"{TokenScheme}:{itemLockInfo.Token:D}"), itemLockInfo.LockRootUri);
        }


        public IEnumerable<ActiveLock> GetActiveLockInfo(IStoreItem item)
        {
            return EmptyActiveLockEntry; 
        }
        private static readonly ActiveLock[] EmptyActiveLockEntry = new ActiveLock[0];
        

        public IEnumerable<LockEntry> GetSupportedLocks(IStoreItem item)
        {
            return EmptyLockEntry;
        }
        private static readonly LockEntry[] EmptyLockEntry = new LockEntry[0];

        public bool IsLocked(IStoreItem item)
        {
            return false;
        }

        public bool HasLock(IStoreItem item, WebDavUri lockToken)
        {
            return false;
        }
    }
}