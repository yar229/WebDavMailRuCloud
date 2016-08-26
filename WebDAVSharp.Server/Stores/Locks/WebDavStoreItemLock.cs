using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Xml;


namespace WebDAVSharp.Server.Stores.Locks
{
    /// <summary>
    /// This class provides the locking functionality.
    /// 
    /// </summary>
    public class WebDavStoreItemLock
    {

        #region Variables
        /// <summary>
        /// Allow Objects to be checked out forever
        /// </summary>
        internal static bool AllowInfiniteCheckouts = false;

        /// <summary>
        /// Max amount of seconds a item can be checkout for.
        /// </summary>
        internal static long MaxCheckOutSeconds = long.MaxValue;
    
        /// <summary>
        /// If false, web dav tells client that it does not support lock.
        /// </summary>
        internal static Boolean LockEnabled = true;

        /// <summary>
        /// Used to store the locks per URI
        /// </summary>
        private static readonly Dictionary<Uri, List<WebDaveStoreItemLockInstance>> ObjectLocks = new Dictionary<Uri, List<WebDaveStoreItemLockInstance>>();

        private static ILockPersister LockPersister { get; set; }

        #endregion

        static WebDavStoreItemLock()
        {
            LockPersister = NullLockPersister.Instance;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="persister"></param>
        public static void SetPersister(ILockPersister persister)
        {
            LockPersister = persister;
        }

        /// <summary>
        /// This function removes any expired locks for the path and if there is no lock in memory it try to use
        /// persister to load one
        /// </summary>
        /// <param name="path"></param>
        private static void LoadAndCleanLocks(Uri path)
        {
            lock (ObjectLocks)
            {
                if (!ObjectLocks.ContainsKey(path))
                {
                    var saved = LockPersister.Load(path);
                    if (saved != null)
                    {
                        ObjectLocks[path] = saved.ToList();
                    }
                    else
                    {
                        ObjectLocks[path] = new List<WebDaveStoreItemLockInstance>();
                    }
                }

                //Cleanup log if necessary
                foreach (
                    WebDaveStoreItemLockInstance ilock in ObjectLocks[path].ToList()
                        .Where(ilock => ilock.ExpirationDate != null && (DateTime)ilock.ExpirationDate < DateTime.Now)
                    )
                {
                    ObjectLocks[path].Remove(ilock);
                }
            }
        }

        /// <summary>
        /// This function will refresh an existing lock.
        /// </summary>
        /// <param name="path">Target URI to the file or folder </param>
        /// <param name="locktoken">The token issued when the lock was established</param>
        /// <param name="requestedlocktimeout">The requested timeout</param>
        /// <param name="requestDocument">Output parameter, returns the Request document that was used when the lock was established.</param>
        /// <returns></returns>
        public static int RefreshLock(Uri path, string locktoken, ref string requestedlocktimeout,
            out XmlDocument requestDocument)
        {
            LoadAndCleanLocks(path);
            //Refreshing an existing lock

            //If a lock doesn't exist then lets just reply with a Precondition Failed.
            //412 (Precondition Failed), with 'lock-token-matches-request-uri' precondition code - The LOCK request was 
            //made with an If header, indicating that the client wishes to refresh the given lock. However, the Request-URI 
            //did not fall within the scope of the lock identified by the token. The lock may have a scope that does not 
            //include the Request-URI, or the lock could have disappeared, or the token may be invalid.
            requestDocument = null;
            if (!ObjectLocks.ContainsKey(path))
                return 412;

            string tmptoken = locktoken;
            lock (ObjectLocks)
            {
                WebDaveStoreItemLockInstance ilock = ObjectLocks[path].FirstOrDefault(d => (d.Token == tmptoken));
                if (ilock == null)
                {
                    WebDavServer.Log.Debug("Lock Refresh Failed , Lock does not exist.");
                    return 412;
                }
                WebDavServer.Log.Debug("Lock Refresh Successful.");
                ilock.RefreshLock(ref requestedlocktimeout);
                requestDocument = ilock.RequestDocument;

                //I changed a lock need to save
                LockPersister.Persist(path, ObjectLocks[path]);
                return (int)HttpStatusCode.OK;
            }
        }

        /// <summary>
        /// Locks the request Path.
        /// </summary>
        /// <param name="path">URI to the item to be locked</param>
        /// <param name="logicalLockKey">Logical lock key taken from document.</param>
        /// <param name="lockscope">The lock Scope used for locking</param>
        /// <param name="locktype">The lock Type used for locking</param>
        /// <param name="lockowner">The owner of the lock</param>
        /// <param name="userAgent">User agent of the request</param>
        /// <param name="requestedlocktimeout">The requested timeout</param>
        /// <param name="locktoken">Out parameter, returns the issued token</param>
        /// <param name="requestDocument">the Request Document</param>
        /// <param name="depth">How deep to lock, 0,1, or infinity</param>
        /// <returns></returns>
        public static int Lock(
            Uri path, 
            String logicalLockKey, 
            WebDavLockScope lockscope, 
            WebDavLockType locktype, 
            string lockowner,
            String userAgent,
            ref string requestedlocktimeout, 
            out string locktoken, 
            XmlDocument requestDocument, 
            int depth)
        {
            LoadAndCleanLocks(path);
            WebDavServer.Log.Debug("Lock Requested Timeout:" + requestedlocktimeout);
            locktoken = string.Empty;
            lock (ObjectLocks)
            {
                /*
            The table below describes the behavior that occurs when a lock request is made on a resource.
            Current State   Shared Lock OK      Exclusive Lock OK
            None	            True	            True
            Shared Lock     	True            	False
            Exclusive Lock	    False	            False*

            Legend: True = lock may be granted. False = lock MUST NOT be granted. *=It is illegal for a principal to request the same lock twice.

            The current lock state of a resource is given in the leftmost column, and lock requests are listed in the first row. The intersection of a row and column gives the result of a lock request. For example, if a shared lock is held on a resource, and an exclusive lock is requested, the table entry is "false", indicating that the lock must not be granted.            
             */


                //if ObjectLocks doesn't contain the path, then this is a new lock and regardless
                if (ObjectLocks[path].Count == 0)
                {
                    //Pay attention, we could have a lock in other path with the very same logical key.
                    var logicalKeyLocks = ObjectLocks.Values
                        .SelectMany(l => l)
                        .Where(l => 
                            l.LogicalLockKey == logicalLockKey && 
                            l.Owner != lockowner &&
                            l.UserAgent != userAgent)
                        .ToList();
                    if (logicalKeyLocks.Count > 0)
                    {
                        //we have some logical lock from different user, if we request exclusive, fail
                        if (lockscope == WebDavLockScope.Exclusive) 
                            return 423;

                        //if lock requested is shared, but anyone of the other has an Exclusive lock fail
                        if (lockscope == WebDavLockScope.Shared &&
                            logicalKeyLocks.Any(l => l.LockScope == WebDavLockScope.Exclusive))
                            return 423;
                    }

                    ObjectLocks[path].Add(new WebDaveStoreItemLockInstance(path,logicalLockKey, userAgent, lockscope, locktype, lockowner,
                        ref requestedlocktimeout, ref locktoken,
                        requestDocument, depth));

                    WebDavServer.Log.DebugFormat("Created New Lock ({0}), URI {1} had no locks. Timeout: {2}", lockscope, path, requestedlocktimeout);

                     LockPersister.Persist(path, ObjectLocks[path]);
                    return (int)HttpStatusCode.OK;
                }

                //The fact that ObjectLocks contains this URI means that there is already a lock on this object,
                //This means the lock fails because you can only have 1 exclusive lock.
                if (lockscope == WebDavLockScope.Exclusive)
                {
                    //TODO: Verify, it seems that the windows client issues multiple lock, if the
                    //lock was already issued to the same identity, we can consider the lock to 
                    //be a refresh.
                    //Check on useragent is needed to verify that the tool is the very same
                    //TODO: we should also check from IP address of caller to be 100% sure.
                    if (ObjectLocks[path].All(l => l.Owner == lockowner && l.UserAgent == userAgent))
                    {
                        //the same owner requested a lock it it should not happen but windows client
                        //issues multiple lock without issuing a UNLOCK (this happens when it issued a DELETE 
                        //probably it assumes that a DELETE also release the LOCK).
                        ObjectLocks[path].Clear(); //clear all old lock, 
                    }
                    else
                    {
                        WebDavServer.Log.DebugFormat("Lock Creation Failed (Exclusive), URI {0} already has a lock.", path);
                        return 423;
                    }
                }

                //If the scope is shared and all other locks on this uri are shared we are ok, otherwise we fail.
                if (lockscope == WebDavLockScope.Shared)
                    if (ObjectLocks[path].Any(itemLock => itemLock.LockScope == WebDavLockScope.Exclusive))
                    {
                        WebDavServer.Log.Debug("Lock Creation Failed (Shared), URI has exclusive lock.");
                        return 423;
                    }
                //423 (Locked), potentially with 'no-conflicting-lock' precondition code - 
                //There is already a lock on the resource that is not compatible with the 
                //requested lock (see lock compatibility table above).

                //If it gets to here, then we are most likely creating another shared lock on the file.

                #region Create New Lock

                ObjectLocks[path].Add(new WebDaveStoreItemLockInstance(path, logicalLockKey, userAgent, lockscope, locktype, lockowner,
                    ref requestedlocktimeout, ref locktoken,
                    requestDocument, depth));

                WebDavServer.Log.Debug("Created New Lock (" + lockscope + "), URI had no locks.  Timeout:" +
                                       requestedlocktimeout);

                #endregion
                LockPersister.Persist(path, ObjectLocks[path]);
                return (int)HttpStatusCode.OK;
            }
        }

        /// <summary>
        /// Unlocks the URI passed if the token matches a lock token in use.
        /// </summary>
        /// <param name="path">URI to resource</param>
        /// <param name="locktoken">Token used to lock.</param>
        /// <param name="owner">Owner.</param>
        /// <returns></returns>
        public static int UnLock(Uri path, string locktoken, string owner)
        {
            LoadAndCleanLocks(path);
            if (string.IsNullOrEmpty(locktoken))
            {
                WebDavServer.Log.DebugFormat("Unlock failed for {0}, No Token!.", path);
                return (int)HttpStatusCode.BadRequest;
            }

            lock (ObjectLocks)
            {
                if (!ObjectLocks.ContainsKey(path))
                {
                    WebDavServer.Log.DebugFormat("Unlock failed, Lock does not exist for {0}!.", path);
                    return (int)HttpStatusCode.Conflict;
                }

                WebDaveStoreItemLockInstance ilock = ObjectLocks[path].FirstOrDefault(d => d.Token == locktoken && d.Owner == owner);
                if (ilock == null)
                {
                    if (WebDavServer.Log.IsDebugEnabled)
                    {
                        StringBuilder sb = new StringBuilder();
                        foreach (var uriLock in ObjectLocks[path])
                        {
                            sb.AppendFormat("token:{0} owner:{1} type: {2}\n", uriLock.Token, uriLock.Owner, uriLock.LockType);
                        }
                        WebDavServer.Log.DebugFormat("Unlock failed: {0} is locked but not whith the same token. All tokens:{1}", path, sb.ToString());
                    }

                    return (int)HttpStatusCode.Conflict;
                }
                    
                //Remove the lock
                ObjectLocks[path].Remove(ilock);

                WebDavServer.Log.DebugFormat("Unlock successful {0} token {1} owner {2}", path, locktoken, owner);
                LockPersister.Persist(path, ObjectLocks[path]);
                return (int)HttpStatusCode.NoContent;
            }
        }

        /// <summary>
        /// Clear all locks on a given path, it is usefulf for deleted object.
        /// </summary>
        /// <param name="path"></param>
        public static void ClearLocks(Uri path)
        {
            lock (ObjectLocks)
            {
                if (ObjectLocks.ContainsKey(path))
                {
                    ObjectLocks.Remove(path);
                }
            }
            LockPersister.Clear(path);
        }

        /// <summary>
        /// Returns all the locks on the path
        /// </summary>
        /// <param name="path">URI to resource</param>
        /// <returns></returns>
        public static List<WebDaveStoreItemLockInstance> GetLocks(Uri path)
        {
            return ObjectLocks.ContainsKey(path) ? ObjectLocks[path].ToList() : new List<WebDaveStoreItemLockInstance>();
        }
    }
}