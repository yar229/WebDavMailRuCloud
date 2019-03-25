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
