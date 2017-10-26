using System.Linq;

using NWebDav.Server.Stores;

namespace NWebDav.Server.Props
{
    /// <summary>
    /// Default implementation to describe the active locks on a resource.
    /// </summary>
    /// <remarks>
    /// This property implementation calls the
    /// <see cref="NWebDav.Server.Locking.ILockingManager.GetActiveLockInfo"/>
    /// of the item's <see cref="IStoreItem.LockingManager"/> to determine the
    /// active locks.
    /// </remarks>
    /// <typeparam name="TEntry">
    /// Store item or collection to which this DAV property applies.
    /// </typeparam>
    public sealed class DavLockDiscoveryDefault<TEntry> : DavLockDiscovery<TEntry> where TEntry : IStoreItem
    {
        /// <summary>
        /// Create an instance of the <see cref="DavLockDiscovery{TEntry}"/>
        /// property that implements the property using the
        /// <see cref="NWebDav.Server.Locking.ILockingManager.GetActiveLockInfo"/> 
        /// method of the item's locking manager.
        /// </summary>
        public DavLockDiscoveryDefault()
        {
            Getter = (httpContext, item) => item.LockingManager.GetActiveLockInfo(item).Select(ali => ali.ToXml());
        }
    }

    /// <summary>
    /// Default implementation to describe the supported locks on a resource.
    /// </summary>
    /// <remarks>
    /// This property implementation calls the
    /// <see cref="NWebDav.Server.Locking.ILockingManager.GetSupportedLocks"/>
    /// of the item's <see cref="IStoreItem.LockingManager"/> to determine the
    /// supported locks.
    /// </remarks>
    /// <typeparam name="TEntry">
    /// Store item or collection to which this DAV property applies.
    /// </typeparam>
    public sealed class DavSupportedLockDefault<TEntry> : DavSupportedLock<TEntry> where TEntry : IStoreItem
    {
        /// <summary>
        /// Create an instance of the <see cref="DavSupportedLock{TEntry}"/>
        /// property that implements the property using the
        /// <see cref="NWebDav.Server.Locking.ILockingManager.GetSupportedLocks"/>
        /// method of the item's locking manager.
        /// </summary>
        public DavSupportedLockDefault()
        {
            Getter = (httpContext, item) => item.LockingManager.GetSupportedLocks(item).Select(sl => sl.ToXml());
        }
    }
}
