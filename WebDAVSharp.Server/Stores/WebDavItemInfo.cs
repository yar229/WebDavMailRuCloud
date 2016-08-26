using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebDAVSharp.Server.Stores
{
    /// <summary>
    /// Grab all information we need on a specific document
    /// </summary>
    public class WebDavItemInfo
    {
        /// <summary>
        /// Creation time of the document
        /// </summary>
        public DateTime CreationTime { get; set; }

        /// <summary>
        /// Last time the document was accessed.
        /// </summary>
        public DateTime LastAccessTime { get; set; }

        /// <summary>
        /// Last time document was written.
        /// </summary>
        public DateTime LastWriteTime { get; set; }
  
    }
}
