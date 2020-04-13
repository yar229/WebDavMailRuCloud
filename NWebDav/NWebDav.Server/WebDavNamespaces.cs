using System.Xml.Linq;

namespace NWebDav.Server
{
    /// <summary>
    /// Defines all the XML namespaces that are typically used in WebDAV
    /// requests and responses.
    /// </summary>
    public static class WebDavNamespaces
    {
        /// <summary>
        /// Main DAV namespace (<c>DAV:</c>).
        /// </summary>
        public static readonly XNamespace DavNs = "DAV:";

        public static readonly XName DavNsActiveLock = DavNs + "activelock";
        public static readonly XName DavNsAllProp = DavNs + "allprop";
        public static readonly XName DavNsDepth = DavNs + "depth";
        public static readonly XName DavNsExclusive = DavNs + "exclusive";
        public static readonly XName DavNsHref = DavNs + "href";
        public static readonly XName DavNsInclude = DavNs + "include";
        public static readonly XName DavNsLockDiscovery = DavNs + "lockdiscovery";
        public static readonly XName DavNsLockEntry = DavNs + "lockentry";
        public static readonly XName DavNsLockInfo = DavNs + "lockinfo";
        public static readonly XName DavNsLockRoot = DavNs + "lockroot";
        public static readonly XName DavNsLockScope = DavNs + "lockscope";
        public static readonly XName DavNsLockToken = DavNs + "locktoken";
        public static readonly XName DavNsLockType = DavNs + "locktype";
        public static readonly XName DavNsMultiStatus = DavNs + "multistatus";
        public static readonly XName DavNsOwner = DavNs + "owner";
        public static readonly XName DavNsProp = DavNs + "prop";
        public static readonly XName DavNsPropFind = DavNs + "propfind";
        public static readonly XName DavNsPropName = DavNs + "propname";
        public static readonly XName DavNsPropStat = DavNs + "propstat";
        public static readonly XName DavNsPropertyUpdate = DavNs + "propertyupdate";
        public static readonly XName DavNsRemove = DavNs + "remove";
        public static readonly XName DavNsResponse = DavNs + "response";
        public static readonly XName DavNsResponseDescription = DavNs + "responsedescription";
        public static readonly XName DavNsSet = DavNs + "set";
        public static readonly XName DavNsShared = DavNs + "shared";
        public static readonly XName DavNsStatus = DavNs + "status";
        public static readonly XName DavNsTimeout = DavNs + "timeout";
        public static readonly XName DavNsWrite = DavNs + "write";
        
        
        

        /// <summary>
        /// Main DAV namespace prefix (<c>D</c>).
        /// Some WebDAV clients don't parse the server generated XML properly
        /// and expect that all DAV nodes use the "D" prefix. Although it is
        /// perfectly legal to use a different namespace prefix, we do use it
        /// to maximize compatibility.
        /// </summary>
        public static readonly string DavNsPrefix = "D";

        /// <summary>
        /// Win32 extension namespace (<c>urn:schemas-microsoft-com:</c>). 
        /// Its primary use is to add date/time and attributes to file-system
        /// based entries. More information can be found at
        /// <see href="https://msdn.microsoft.com/en-us/library/jj557737(v=office.12).aspx"/>.
        /// </summary>
        public static readonly XNamespace Win32Ns = "urn:schemas-microsoft-com:";

        /// <summary>
        /// Win32 namespace prefix (<c>Z</c>). Some WebDAV clients don't parse
        /// the server generated XML properly and expect that the Win32 nodes
        /// use the <c>Z</c> prefix. Although it is perfectly legal to use a
        /// different namespace prefix, we do use it to maximize compatibility.
        /// </summary>
        public static readonly string Win32NsPrefix = "Z";

        /// <summary>
        /// WebDAV replication namespace (<c>http://schemas.microsoft.com/repl</c>).
        /// It defines fields used for WebDAV replication. More information can
        /// be found at
        /// <see href="https://msdn.microsoft.com/en-us/library/ms875925(v=exchg.65).aspx"/>.
        /// </summary>
        public static readonly XNamespace ReplNs = "http://schemas.microsoft.com/repl/";

        /// <summary>
        /// Office namespace (<c>urn:schemas-microsoft-com:office:office</c>).
        /// It defines properties for use with Microsoft Office applications.
        /// More information can be found at
        /// <see href="https://msdn.microsoft.com/en-us/library/ms875215(v=exchg.65).aspx"/>.
        /// </summary>
        public static readonly XNamespace OfficeNs = "urn:schemas-microsoft-com:office:office";
    }
}
