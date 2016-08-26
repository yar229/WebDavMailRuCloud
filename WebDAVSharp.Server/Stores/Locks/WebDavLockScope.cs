using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebDAVSharp.Server.Stores.Locks
{
    /// <summary>
    /// Possible scopes for locks.
    /// </summary>
    public enum WebDavLockScope
    {
        /// <summary>
        /// Can only have one exclusive Lock
        /// </summary>
        Exclusive,

        /// <summary>
        /// Can be many.
        /// </summary>
        Shared
    }
}
