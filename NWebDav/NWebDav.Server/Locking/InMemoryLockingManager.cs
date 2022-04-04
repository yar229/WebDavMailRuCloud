using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

using NWebDav.Server.Stores;

namespace NWebDav.Server.Locking
{
    // TODO: Remove auto-expired locks
    // TODO: Add support for recursive locks
    public class InMemoryLockingManager : ILockingManager
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

        private class ItemLockList : List<ItemLockInfo>
        {
        }

        private class ItemLockTypeDictionary : Dictionary<LockType, ItemLockList>
        {
        }

        private const string TokenScheme = "opaquelocktoken";

        private readonly IDictionary<string, ItemLockTypeDictionary> _itemLocks = new Dictionary<string, ItemLockTypeDictionary>();

        private static readonly LockEntry[] s_supportedLocks =
        {
            new LockEntry(LockScope.Exclusive, LockType.Write),
            new LockEntry(LockScope.Shared, LockType.Write)
        };

        public LockResult Lock(IStoreItem item, LockType lockType, LockScope lockScope, XElement owner, WebDavUri lockRootUri, bool recursive, IEnumerable<int> timeouts)
        {
            // Determine the expiration based on the first time-out
            var timeout = timeouts.Cast<int?>().FirstOrDefault();

            // Determine the item's key
            var key = item.UniqueKey;

            lock (_itemLocks)
            {
                // Make sure the item is in the dictionary
                if (!_itemLocks.TryGetValue(key, out var itemLockTypeDictionary))
                    _itemLocks.Add(key, itemLockTypeDictionary = new ItemLockTypeDictionary());

                // Make sure there is already a lock-list for this type
                if (!itemLockTypeDictionary.TryGetValue(lockType, out var itemLockList))
                {
                    // Create a new lock-list
                    itemLockTypeDictionary.Add(lockType, itemLockList = new ItemLockList());
                }
                else
                {
                    // Check if there is already an exclusive lock
                    if (itemLockList.Any(l => l.Scope == LockScope.Exclusive))
                        return new LockResult(DavStatusCode.Locked);
                }

                // Create the lock info object
                var itemLockInfo = new ItemLockInfo(item, lockType, lockScope, lockRootUri, recursive, owner, timeout ?? -1);

                // Add the lock
                itemLockList.Add(itemLockInfo);

                // Return the active lock
                return new LockResult(DavStatusCode.Ok, GetActiveLockInfo(itemLockInfo));
            }
        }

        public DavStatusCode Unlock(IStoreItem item, WebDavUri lockTokenUri)
        {
            // Determine the actual lock token
            var lockToken = GetTokenFromLockToken(lockTokenUri);
            if (lockToken == null)
                return DavStatusCode.PreconditionFailed;

            // Determine the item's key
            var key = item.UniqueKey;

            lock (_itemLocks)
            {
                // Make sure the item is in the dictionary
                if (!_itemLocks.TryGetValue(key, out var itemLockTypeDictionary))
                    return DavStatusCode.PreconditionFailed;

                // Scan both the dictionaries for the token
                foreach (var kv in itemLockTypeDictionary)
                {
                    var itemLockList = kv.Value;

                    // Remove this lock from the list
                    for (var i = 0; i < itemLockList.Count; ++i)
                    {
                        if (itemLockList[i].Token == lockToken.Value)
                        {
                            // Remove the item
                            itemLockList.RemoveAt(i);

                            // Check if there are any locks left for this type
                            if (!itemLockList.Any())
                            {
                                // Remove the type
                                itemLockTypeDictionary.Remove(kv.Key);

                                // Check if there are any types left
                                if (!itemLockTypeDictionary.Any())
                                    _itemLocks.Remove(key);
                            }

                            // Lock has been removed
                            return DavStatusCode.NoContent;
                        }
                    }
                }
            }

            // Item cannot be unlocked (token cannot be found)
            return DavStatusCode.PreconditionFailed;
        }

        public LockResult RefreshLock(IStoreItem item, bool recursiveLock, IEnumerable<int> timeouts, WebDavUri lockTokenUri)
        {
            // Determine the actual lock token
            var lockToken = GetTokenFromLockToken(lockTokenUri);
            if (lockToken == null)
                return new LockResult(DavStatusCode.PreconditionFailed);

            // Determine the item's key
            var key = item.UniqueKey;

            lock (_itemLocks)
            {
                // Make sure the item is in the dictionary
                if (!_itemLocks.TryGetValue(key, out var itemLockTypeDictionary))
                    return new LockResult(DavStatusCode.PreconditionFailed);

                // Scan both the dictionaries for the token
                foreach (var kv in itemLockTypeDictionary)
                {
                    // Refresh the lock
                    var itemLockInfo = kv.Value.FirstOrDefault(lt => lt.Token == lockToken.Value && !lt.IsExpired);
                    if (itemLockInfo != null)
                    {
                        // Determine the expiration based on the first time-out
                        var timeout = timeouts.Cast<int?>().FirstOrDefault() ?? itemLockInfo.Timeout;
                        itemLockInfo.RefreshExpiration(timeout);

                        // Return the active lock
                        return new LockResult(DavStatusCode.Ok, GetActiveLockInfo(itemLockInfo));
                    }
                }
            }

            // Item cannot be unlocked (token cannot be found)
            return new LockResult(DavStatusCode.PreconditionFailed);
        }

        public IEnumerable<ActiveLock> GetActiveLockInfo(IStoreItem item)
        {
            // Determine the item's key
            var key = item.UniqueKey;

            lock (_itemLocks)
            {
                // Make sure the item is in the dictionary
                if (!_itemLocks.TryGetValue(key, out var itemLockTypeDictionary))
                    return Array.Empty<ActiveLock>();

                // Return all non-expired locks
                return itemLockTypeDictionary.SelectMany(kv => kv.Value).Where(l => !l.IsExpired).Select(GetActiveLockInfo).ToList();
            }
        }

        public IEnumerable<LockEntry> GetSupportedLocks(IStoreItem item)
        {
            // We support both shared and exclusive locks for items and collections
            return s_supportedLocks;
        }

        public bool IsLocked(IStoreItem item)
        {
            // Determine the item's key
            var key = item.UniqueKey;

            lock (_itemLocks)
            {
                // Make sure the item is in the dictionary
                if (_itemLocks.TryGetValue(key, out var itemLockTypeDictionary))
                {
                    foreach (var kv in itemLockTypeDictionary)
                    {
                        if (kv.Value.Any(li => !li.IsExpired))
                            return true;
                    }
                }
            }

            // No lock
            return false;
        }

        public bool HasLock(IStoreItem item, WebDavUri lockTokenUri)
        {
            // If no lock is specified, then we should abort
            if (lockTokenUri == null)
                return false;

            // Determine the item's key
            var key = item.UniqueKey;

            // Determine the actual lock token
            var lockToken = GetTokenFromLockToken(lockTokenUri);
            if (lockToken == null)
                return false;

            lock (_itemLocks)
            {
                // Make sure the item is in the dictionary
                if (!_itemLocks.TryGetValue(key, out var itemLockTypeDictionary))
                    return false;

                // Scan both the dictionaries for the token
                foreach (var kv in itemLockTypeDictionary)
                {
                    // Refresh the lock
                    var itemLockInfo = kv.Value.FirstOrDefault(lt => lt.Token == lockToken.Value && !lt.IsExpired);
                    if (itemLockInfo != null)
                        return true;
                }
            }

            // No lock
            return false;
        }

        private static ActiveLock GetActiveLockInfo(ItemLockInfo itemLockInfo)
        {
            return new ActiveLock(itemLockInfo.Type, itemLockInfo.Scope, itemLockInfo.Recursive ? int.MaxValue : 0, itemLockInfo.Owner, itemLockInfo.Timeout, new WebDavUri($"{TokenScheme}:{itemLockInfo.Token:D}"), itemLockInfo.LockRootUri);
        }

        private static Guid? GetTokenFromLockToken(WebDavUri lockTokenUri)
        {
            // We should always use opaquetokens
            if (LockTokenUri.Scheme(lockTokenUri) != TokenScheme)
                return null;

            // Parse the token
            if (!Guid.TryParse(LockTokenUri.LocalPath(lockTokenUri), out var lockToken))
                return null;

            // Return the token
            return lockToken;
        }

        private static class LockTokenUri
        {
            public static string Scheme(WebDavUri lockTokenUri)
            {
                // LockTokenUri does not contains "//" ?
                ///<see cref="ImMemoryLockingManager.GetTokenFromLockToken"/>
                int pos = lockTokenUri.AbsoluteUri.IndexOf(":", StringComparison.OrdinalIgnoreCase);
                
                return lockTokenUri.AbsoluteUri.Substring(0, pos);
            }

            public static string LocalPath(WebDavUri lockTokenUri)
            {
                // LockTokenUri does not contains "//" ?
                ///<see cref="ImMemoryLockingManager.GetTokenFromLockToken"/>
                int pos = lockTokenUri.AbsoluteUri.IndexOf(":", StringComparison.OrdinalIgnoreCase);
                
                return lockTokenUri.AbsoluteUri.Substring(pos + 1);
            }
        }
    }
}
