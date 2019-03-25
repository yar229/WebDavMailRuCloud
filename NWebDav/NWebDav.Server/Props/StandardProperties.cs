using System.Xml.Linq;

using NWebDav.Server.Stores;

namespace NWebDav.Server.Props
{
    /// <summary>
    /// Records the time and date the resource was created (ISO 8601 format).
    /// </summary>
    /// <remarks>
    /// <para>
    /// The creationdate property should be defined on all DAV compliant
    /// resources. If present, it contains a timestamp of the moment when
    /// the resource was created (i.e., the moment it had non-null state).
    /// </para>
    /// </remarks>
    /// <typeparam name="TEntry">
    /// Store item or collection to which this DAV property applies.
    /// </typeparam>
    public class DavCreationDate<TEntry> : DavIso8601Date<TEntry> where TEntry : IStoreItem
    {
        /// <summary>
        /// Name of the property (static).
        /// </summary>
        // ReSharper disable once StaticMemberInGenericType
        public static readonly XName PropertyName = WebDavNamespaces.DavNs + "creationdate";

        /// <summary>
        /// Name of the property.
        /// </summary>
        public override XName Name => PropertyName;
    }

    /// <summary>
    /// Provides a name for the resource that is suitable for presentation to a
    /// user.
    /// </summary>
    /// <remarks>
    /// The displayname property should be defined on all DAV compliant
    /// resources. If present, the property contains a description of the
    /// resource that is suitable for presentation to a user.
    /// </remarks>
    /// <typeparam name="TEntry">
    /// Store item or collection to which this DAV property applies.
    /// </typeparam>
    public class DavDisplayName<TEntry> : DavString<TEntry> where TEntry : IStoreItem
    {
        /// <summary>
        /// Name of the property (static).
        /// </summary>
        // ReSharper disable once StaticMemberInGenericType
        public static readonly XName PropertyName = WebDavNamespaces.DavNs + "displayname";

        /// <summary>
        /// Name of the property.
        /// </summary>
        public override XName Name => PropertyName;
    }

    /// <summary>
    /// Contains the Content-Language header returned by a GET without accept
    /// headers.
    /// </summary>
    /// <remarks>
    /// The getcontentlanguage property must be defined on any DAV compliant
    /// resource that returns the Content-Language header on a GET. The 
    /// format of a language-tag is defined in 
    /// <see href="http://tools.ietf.org/html/rfc2068#section-14.13">section 14.13 of RFC 2068</see>.
    /// </remarks>
    /// <typeparam name="TEntry">
    /// Store item or collection to which this DAV property applies.
    /// </typeparam>
    public class DavGetContentLanguage<TEntry> : DavString<TEntry> where TEntry : IStoreItem
    {
        /// <summary>
        /// Name of the property (static).
        /// </summary>
        // ReSharper disable once StaticMemberInGenericType
        public static readonly XName PropertyName = WebDavNamespaces.DavNs + "getcontentlanguage";

        /// <summary>
        /// Name of the property.
        /// </summary>
        public override XName Name => PropertyName;
    }

    /// <summary>
    /// Contains the Content-Length header returned by a GET without accept
    /// headers.
    /// </summary>
    /// <remarks>
    /// The getcontentlength property must be defined on any DAV compliant
    /// resource that returns the Content-Length header in response to a GET.
    /// </remarks>
    /// <typeparam name="TEntry">
    /// Store item or collection to which this DAV property applies.
    /// </typeparam>
    public class DavGetContentLength<TEntry> : DavInt64<TEntry> where TEntry : IStoreItem
    {
        /// <summary>
        /// Name of the property (static).
        /// </summary>
        // ReSharper disable once StaticMemberInGenericType
        public static readonly XName PropertyName = WebDavNamespaces.DavNs + "getcontentlength";

        /// <summary>
        /// Name of the property.
        /// </summary>
        public override XName Name => PropertyName;
    }

    /// <summary>
    /// Contains the Content-Type header returned by a GET without accept
    /// headers.
    /// </summary>
    /// <remarks>
    /// This getcontenttype property must be defined on any DAV compliant
    /// resource that returns the Content-Type header in response to a GET.
    /// Media types are defined in
    /// <see href="http://tools.ietf.org/html/rfc2068#section-3.7">section 3.7 of RFC 2068</see>.
    /// </remarks>
    /// <typeparam name="TEntry">
    /// Store item or collection to which this DAV property applies.
    /// </typeparam>
    public class DavGetContentType<TEntry> : DavString<TEntry> where TEntry : IStoreItem
    {
        /// <summary>
        /// Name of the property (static).
        /// </summary>
        // ReSharper disable once StaticMemberInGenericType
        public static readonly XName PropertyName = WebDavNamespaces.DavNs + "getcontenttype";

        /// <summary>
        /// Name of the property.
        /// </summary>
        public override XName Name => PropertyName;
    }

    /// <summary>
    /// Contains the ETag header returned by a GET without accept headers.
    /// </summary>
    /// <remarks>
    /// The getetag property must be defined on any DAV compliant resource that
    /// returns the Etag header. Entity tags are defined in
    /// <see href="http://tools.ietf.org/html/rfc2068#section-3.11">secion 3.11 of RFC 2068</see>.
    /// </remarks>
    /// <typeparam name="TEntry">
    /// Store item or collection to which this DAV property applies.
    /// </typeparam>
    public class DavGetEtag<TEntry> : DavString<TEntry> where TEntry : IStoreItem
    {
        /// <summary>
        /// Name of the property (static).
        /// </summary>
        // ReSharper disable once StaticMemberInGenericType
        public static readonly XName PropertyName = WebDavNamespaces.DavNs + "getetag";

        /// <summary>
        /// Name of the property.
        /// </summary>
        public override XName Name => PropertyName;
    }

    /// <summary>
    /// Contains the Last-Modified header returned by a GET method without
    /// accept headers (HTTP date).
    /// </summary>
    /// <remarks>
    /// <para>
    /// Note that the last-modified date on a resource may reflect changes in
    /// any part of the state of the resource, not necessarily just a change
    /// to the response to the GET method. For example, a change in a property
    /// may cause the last-modified date to change. The getlastmodified
    /// property must be defined on any DAV compliant resource that returns
    /// the Last-Modified header in response to a GET.
    /// </para>
    /// <para>
    /// Note that this property returns the date in HTTP format, which is
    /// different from the standard XML representation as in ISO 8601.
    /// </para>
    /// </remarks>
    /// <typeparam name="TEntry">
    /// Store item or collection to which this DAV property applies.
    /// </typeparam>
    public class DavGetLastModified<TEntry> : DavRfc1123Date<TEntry> where TEntry : IStoreItem
    {
        /// <summary>
        /// Name of the property (static).
        /// </summary>
        // ReSharper disable once StaticMemberInGenericType
        public static readonly XName PropertyName = WebDavNamespaces.DavNs + "getlastmodified";
     
        /// <summary>
        /// Name of the property.
        /// </summary>
        public override XName Name => PropertyName;
    }

    /// <summary>
    /// Describes the active locks on a resource.
    /// </summary>
    /// <remarks>
    /// This property returns a listing of who has a lock, what type of lock he
    /// has, the timeout type and the time remaining on the timeout, and the
    /// associated lock token. The server is free to withhold any or all of
    /// this information if the requesting principal does not have sufficient
    /// access rights to see the requested data.
    /// </remarks>
    /// <typeparam name="TEntry">
    /// Store item or collection to which this DAV property applies.
    /// </typeparam>
    public class DavLockDiscovery<TEntry> : DavXElementArray<TEntry> where TEntry : IStoreItem
    {
        /// <summary>
        /// Name of the property (static).
        /// </summary>
        // ReSharper disable once StaticMemberInGenericType
        public static readonly XName PropertyName = WebDavNamespaces.DavNs + "lockdiscovery";

        /// <summary>
        /// Name of the property.
        /// </summary>
        public override XName Name => PropertyName;
    }

    /// <summary>
    /// Specifies the nature of the resource.
    /// </summary>
    /// <remarks>
    /// The resourcetype property must be defined on all DAV compliant
    /// resources. The default value is empty.
    /// </remarks>
    /// <typeparam name="TEntry">
    /// Store item or collection to which this DAV property applies.
    /// </typeparam>
    public class DavGetResourceType<TEntry> : DavXElementArray<TEntry> where TEntry : IStoreItem
    {
        /// <summary>
        /// Name of the property (static).
        /// </summary>
        // ReSharper disable once StaticMemberInGenericType
        public static readonly XName PropertyName = WebDavNamespaces.DavNs + "resourcetype";

        /// <summary>
        /// Name of the property.
        /// </summary>
        public override XName Name => PropertyName;
    }

    /// <summary>
    /// The destination of the source link identifies the resource that
    /// contains the unprocessed source of the link's source.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The source of the link (src) is typically the URI of the output
    /// resource on which the link is defined, and there is typically only one
    /// destination (dst) of the link, which is the URI where the unprocessed
    /// source of the resource may be accessed. When more than one link
    /// destination exists, this specification asserts no policy on ordering.
    /// </para>
    /// <para>
    /// See 
    /// <see href="http://www.webdav.org/specs/rfc2518.html#rfc.section.13.10">section 13.10 of RFC 2418</see>
    /// for more information and an example of a source result.
    /// </para>
    /// </remarks>
    /// <typeparam name="TEntry">
    /// Store item or collection to which this DAV property applies.
    /// </typeparam>
    public class DavSource<TEntry> : DavXElement<TEntry> where TEntry : IStoreItem
    {
        /// <summary>
        /// Name of the property (static).
        /// </summary>
        // ReSharper disable once StaticMemberInGenericType
        public static readonly XName PropertyName = WebDavNamespaces.DavNs + "source";

        /// <summary>
        /// Name of the property.
        /// </summary>
        public override XName Name => PropertyName;
    }

    /// <summary>
    /// Provides a listing of the lock capabilities supported by the resource.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This property of a resource returns a listing of the combinations of
    /// scope and access types which may be specified in a lock request on the
    /// resource. Note that the actual contents are themselves controlled by
    /// access controls so a server is not required to provide information the
    /// client is not authorized to see.
    /// </para>
    /// <para>
    /// See 
    /// <see href="http://www.webdav.org/specs/rfc2518.html#rfc.section.13.11.1">section 13.11 of RFC 2418</see>
    /// for more information and an example of a source result.
    /// </para>
    /// </remarks>
    /// <typeparam name="TEntry">
    /// Store item or collection to which this DAV property applies.
    /// </typeparam>
    public class DavSupportedLock<TEntry> : DavXElementArray<TEntry> where TEntry : IStoreItem
    {
        /// <summary>
        /// Name of the property (static).
        /// </summary>
        // ReSharper disable once StaticMemberInGenericType
        public static readonly XName PropertyName = WebDavNamespaces.DavNs + "supportedlock";

        /// <summary>
        /// Name of the property.
        /// </summary>
        public override XName Name => PropertyName;
    }
}
