using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebDAVSharp.Server.Stores.Locks
{
    /// <summary>
    /// Specifies the access type of a lock. At present, this specification only defines one lock type, the write lock.
    /// </summary>
    public enum WebDavLockType
    {
        /// <summary>
        /// A Write Lock type
        /// </summary>
        Write,
    }
}
