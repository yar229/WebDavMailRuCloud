using System;
using System.Collections.Generic;
using System.Xml.Linq;

using NWebDav.Server.Stores;

namespace NWebDav.Server.Locking
{
    public struct LockResult
    {
        public DavStatusCode Result { get; }
        public ActiveLock? Lock { get; }

        public LockResult(DavStatusCode result, ActiveLock? @lock = null)
        {
            Result = result;
            Lock = @lock;
        }
    }

    // TODO: Call the locking methods from the handlers
    public interface ILockingManager
    {
        LockResult Lock(IStoreItem item, LockType lockType, LockScope lockScope, XElement owner, WebDavUri lockRootUri, bool recursiveLock, IEnumerable<int> timeouts);
        DavStatusCode Unlock(IStoreItem item, WebDavUri token);
        LockResult RefreshLock(IStoreItem item, bool recursiveLock, IEnumerable<int> timeouts, WebDavUri lockTokenUri);

        IEnumerable<ActiveLock> GetActiveLockInfo(IStoreItem item);
        IEnumerable<LockEntry> GetSupportedLocks(IStoreItem item);

        bool IsLocked(IStoreItem item);
        bool HasLock(IStoreItem item, WebDavUri lockToken);
    }
}
