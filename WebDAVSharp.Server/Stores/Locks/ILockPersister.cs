using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebDAVSharp.Server.Stores.Locks
{
    /// <summary>
    /// 
    /// </summary>
    public interface ILockPersister
    {
        /// <summary>
        /// Save a lock for a given uri.
        /// </summary>
        /// <param name="uri"></param>
        /// <param name="locks"></param>
        void Persist(Uri uri, IEnumerable<WebDaveStoreItemLockInstance> locks);

        /// <summary>
        /// Reload locks for a given uri
        /// </summary>
        /// <param name="uri"></param>
        /// <returns></returns>
        IEnumerable<WebDaveStoreItemLockInstance> Load(Uri uri);

        /// <summary>
        /// Clear all locks for given path.
        /// </summary>
        /// <param name="path"></param>
        void Clear(Uri path);
    }

    /// <summary>
    /// 
    /// </summary>
    public class NullLockPersister : ILockPersister
    {
        /// <summary>
        /// Singleton
        /// </summary>
        public static NullLockPersister Instance;

        static NullLockPersister()
        {
            Instance = new NullLockPersister();
        }

        private NullLockPersister()
        {

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="path"></param>
        public void Clear(Uri path)
        {
            
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="uri"></param>
        /// <returns></returns>
        public IEnumerable<WebDaveStoreItemLockInstance> Load(Uri uri)
        {
            return new WebDaveStoreItemLockInstance[] { };
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="uri"></param>
        /// <param name="locks"></param>
        public void Persist(Uri uri, IEnumerable<WebDaveStoreItemLockInstance> locks)
        {

        }
    }

}
