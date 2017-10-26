using System.Xml.Linq;

using NWebDav.Server.Stores;

namespace NWebDav.Server.Props
{
    /// <summary>
    /// Specifies the number of contained resources.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This property identifies the number of resources contained in a given
    /// collection.It contains a single integer value with the count of
    /// contained resources. This property includes child collections in the
    /// count.
    /// </para>
    /// <para>
    /// This is an extend WebDAV collection property as defined in the
    /// <see href="https://tools.ietf.org/html/draft-hopmann-collection-props-00#section-1.2">
    /// draft document of Alex Hopmann (Microsoft) and Lisa Lippert (Microsoft)
    /// </see>.
    /// </para>
    /// </remarks>
    /// <typeparam name="TEntry">
    /// Store item or collection to which this DAV property applies.
    /// </typeparam>
    public class DavExtCollectionChildCount<TEntry> : DavInt32<TEntry> where TEntry : IStoreItem
    {
        /// <summary>
        /// Name of the property (static).
        /// </summary>
        // ReSharper disable once StaticMemberInGenericType
        public static readonly XName PropertyName = WebDavNamespaces.DavNs + "childcount";

        /// <summary>
        /// Name of the property.
        /// </summary>
        public override XName Name => PropertyName;
    }

    /// <summary>
    /// Specifies the default document for a collection.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This property contains a URL that identifies the default document for
    /// a collection. This is intended for collection owners to be able to set
    /// a default document, for example index.html or default.html.If this
    /// property is absent, other means must be found to determine the default
    /// document. If this property is present but null, the collection does
    /// not have a default document and the collection member listing should
    /// be used (or nothing).
    /// </para>
    /// <para>
    /// This is an extend WebDAV collection property as defined in the
    /// <see href="https://tools.ietf.org/html/draft-hopmann-collection-props-00#section-1.2">
    /// draft document of Alex Hopmann (Microsoft) and Lisa Lippert (Microsoft)
    /// </see>.
    /// </para>
    /// </remarks>
    /// <typeparam name="TEntry">
    /// Store item or collection to which this DAV property applies.
    /// </typeparam>
    public class DavExtCollectionDefaultDocument<TEntry> : DavString<TEntry> where TEntry : IStoreItem
    {
        /// <summary>
        /// Name of the property (static).
        /// </summary>
        // ReSharper disable once StaticMemberInGenericType
        public static readonly XName PropertyName = WebDavNamespaces.DavNs + "defaultdocument";

        /// <summary>
        /// Name of the property.
        /// </summary>
        public override XName Name => PropertyName;
    }

    /// <summary>
    /// Specifies a unique identifier for this resource.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This property contains a globally unique string that identifies this
    /// resource. This property MUST be unique across the entire Internet.
    /// The id property does not change if the resource changes. This
    /// property is intended to aid in recognition of a resource even when
    /// moved, updated or renamed. The value of this property is a URI.
    /// </para>
    /// <para>
    /// This is an extend WebDAV collection property as defined in the
    /// <see href="https://tools.ietf.org/html/draft-hopmann-collection-props-00#section-1.2">
    /// draft document of Alex Hopmann (Microsoft) and Lisa Lippert (Microsoft)
    /// </see>.
    /// </para>
    /// </remarks>
    /// <typeparam name="TEntry">
    /// Store item or collection to which this DAV property applies.
    /// </typeparam>
    public class DavExtCollectionId<TEntry> : DavUri<TEntry> where TEntry : IStoreItem
    {
        /// <summary>
        /// Name of the property (static).
        /// </summary>
        // ReSharper disable once StaticMemberInGenericType
        public static readonly XName PropertyName = WebDavNamespaces.DavNs + "id";

        /// <summary>
        /// Name of the property.
        /// </summary>
        public override XName Name => PropertyName;
    }

    /// <summary>
    /// Specifies whether or not a collection should appear as a folder.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This property identifies whether or not a collection should appear as
    /// a folder. If true (or absent), the collection should be displayed as a
    /// folder. If false, the collection should NOT be displayed as a folder.
    /// For example, a structured document should have "isfolder" set to false.
    /// </para>
    /// <para>
    /// This is an extend WebDAV collection property as defined in the
    /// <see href="https://tools.ietf.org/html/draft-hopmann-collection-props-00#section-1.2">
    /// draft document of Alex Hopmann (Microsoft) and Lisa Lippert (Microsoft)
    /// </see>.
    /// </para>
    /// </remarks>
    /// <typeparam name="TEntry">
    /// Store item or collection to which this DAV property applies.
    /// </typeparam>
    public class DavExtCollectionIsFolder<TEntry> : DavBoolean<TEntry> where TEntry : IStoreItem
    {
        /// <summary>
        /// Name of the property (static).
        /// </summary>
        // ReSharper disable once StaticMemberInGenericType
        public static readonly XName PropertyName = WebDavNamespaces.DavNs + "isfolder";

        /// <summary>
        /// Name of the property.
        /// </summary>
        public override XName Name => PropertyName;
    }

    /// <summary>
    /// Specifies whether or not a resource is hidden.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This property identifies whether or not a resource is hidden. This can
    /// be considered a hint to the client UI: under normal conditions, for
    /// non-expert users, hidden files should not be exposed to users. The
    /// server may omit the hidden resource from some presentational listings,
    /// otherwise the client is responsible for removing hidden resources when
    /// displaying to the user. If this property is absent, the collection is
    /// not hidden. Since this property provides no actual form of protection
    /// to the resources, this MUST NOT be used as a form of access control
    /// and should only be used for presentation purposes.
    /// </para>
    /// <para>
    /// Many file systems have the option to hide files from the user, but the
    /// user can, with special commands, override the hiding.
    /// </para>
    /// <para>
    /// This is an extend WebDAV collection property as defined in the
    /// <see href="https://tools.ietf.org/html/draft-hopmann-collection-props-00#section-1.2">
    /// draft document of Alex Hopmann (Microsoft) and Lisa Lippert (Microsoft)
    /// </see>.
    /// </para>
    /// </remarks>
    /// <typeparam name="TEntry">
    /// Store item or collection to which this DAV property applies.
    /// </typeparam>
    public class DavExtCollectionIsHidden<TEntry> : DavBoolean<TEntry> where TEntry : IStoreItem
    {
        /// <summary>
        /// Name of the property (static).
        /// </summary>
        // ReSharper disable once StaticMemberInGenericType
        public static readonly XName PropertyName = WebDavNamespaces.DavNs + "ishidden";

        /// <summary>
        /// Name of the property.
        /// </summary>
        public override XName Name => PropertyName;
    }

    /// <summary>
    /// Specifies whether the resource is a structured document.
    /// </summary>
    /// <remarks>
    /// <para>
    /// A structured document is a collection (iscollection should also be
    /// true), so COPY, MOVE and DELETE work as for a collection. The
    /// structured document may behave at times like a document. For example,
    /// clients may wish to display the resource as a document rather than as a
    /// collection. If this property is absent, the collection is not a
    /// structured document.
    /// </para>
    /// <para>
    /// This property can also be considered a hint for the client UI: if the
    /// value of "isstructureddocument" is true, then the client UI may display
    /// this to the user as if it were single document. This can be very useful
    /// when the default document of a collection is an HTML page with a bunch
    /// of images which are the other resources in the collection: only the
    /// default document is intended to be viewed as a document, so the entire
    /// structure can appear as one document. A Structured document may contain
    /// collections.  A structured document must have a default document (if
    /// the "defaultdocument" property is absent, the default document is
    /// assumed by the client to be index.html).
    /// </para>
    /// <para>
    /// This is an extend WebDAV collection property as defined in the
    /// <see href="https://tools.ietf.org/html/draft-hopmann-collection-props-00#section-1.2">
    /// draft document of Alex Hopmann (Microsoft) and Lisa Lippert (Microsoft)
    /// </see>.
    /// </para>
    /// </remarks>
    /// <typeparam name="TEntry">
    /// Store item or collection to which this DAV property applies.
    /// </typeparam>
    public class DavExtCollectionIsStructuredDocument<TEntry> : DavBoolean<TEntry> where TEntry : IStoreItem
    {
        /// <summary>
        /// Name of the property (static).
        /// </summary>
        // ReSharper disable once StaticMemberInGenericType
        public static readonly XName PropertyName = WebDavNamespaces.DavNs + "isstructureddocument";

        /// <summary>
        /// Name of the property.
        /// </summary>
        public override XName Name => PropertyName;
    }

    /// <summary>
    /// Identifies whether this collection contains any collections which are
    /// folders (see <see cref="DavExtCollectionIsFolder{TEntry}"/>).
    /// </summary>
    /// <remarks>
    /// <para>
    /// This property identifies whether or not a folder contains sub-folders,
    /// from the point of view of client display. Sub-folders are child
    /// collections for which "isfolder" is true.
    /// </para>
    /// <para>
    /// If absent, nothing can be guessed about whether the collection has
    /// sub-folders. This property is useful for the efficient display of
    /// hierarchy user interfaces. If "hassubs" is true, then "isfolder" should
    /// also be true so that clients understand that the folder can be expanded
    /// to view its children.
    /// </para>
    /// <para>
    /// This is an extend WebDAV collection property as defined in the
    /// <see href="https://tools.ietf.org/html/draft-hopmann-collection-props-00#section-1.2">
    /// draft document of Alex Hopmann (Microsoft) and Lisa Lippert (Microsoft)
    /// </see>.
    /// </para>
    /// </remarks>
    /// <typeparam name="TEntry">
    /// Store item or collection to which this DAV property applies.
    /// </typeparam>
    public class DavExtCollectionHasSubs<TEntry> : DavBoolean<TEntry> where TEntry : IStoreItem
    {
        /// <summary>
        /// Name of the property (static).
        /// </summary>
        // ReSharper disable once StaticMemberInGenericType
        public static readonly XName PropertyName = WebDavNamespaces.DavNs + "hassubs";

        /// <summary>
        /// Name of the property.
        /// </summary>
        public override XName Name => PropertyName;
    }

    /// <summary>
    /// Identifies whether this collection allows child collections to be
    /// created.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This property identifies whether or not a collection allows child
    /// collections to be created. True indicates that the collection does not
    /// allow child collections). While this data is redundant with that
    /// returned by the OPTIONS method, providing this information as a
    /// property allows better performance since the client can verify the
    /// behavior ahead of time without having to issue an individual OPTIONS
    /// request on every collection it encounters. If absent, nothing can be
    /// guessed about whether the collection allows sub-collections.
    /// </para>
    /// <para>
    /// This property can also be considered to be a hint to the UI about
    /// displaying options to the user (the UI might eliminate the option to
    /// create a child collection). It is different from a "create child"
    /// access right, because the client UI may want to display a "create
    /// child collection" option without trying to find out if the user has
    /// permissions. This property can be used to suggest that creating
    /// child collections just doesn't make sense on this collection no matter
    /// what rights the user has. It is most useful on special-purpose
    /// collections, such as a deleted files collection or a collection which
    /// represents a device such as a printer.
    /// </para>
    /// <para>
    /// This property should not be construed as meaning that sub-collections
    /// do not already exist on the collection. It simply prevents new
    /// collections from being created by the client.
    /// </para>
    /// <para>
    /// This is an extend WebDAV collection property as defined in the
    /// <see href="https://tools.ietf.org/html/draft-hopmann-collection-props-00#section-1.2">
    /// draft document of Alex Hopmann (Microsoft) and Lisa Lippert (Microsoft)
    /// </see>.
    /// </para>
    /// </remarks>
    /// <typeparam name="TEntry">
    /// Store item or collection to which this DAV property applies.
    /// </typeparam>
    public class DavExtCollectionNoSubs<TEntry> : DavBoolean<TEntry> where TEntry : IStoreItem
    {
        /// <summary>
        /// Name of the property (static).
        /// </summary>
        // ReSharper disable once StaticMemberInGenericType
        public static readonly XName PropertyName = WebDavNamespaces.DavNs + "nosubs";

        /// <summary>
        /// Name of the property.
        /// </summary>
        public override XName Name => PropertyName;
    }

    /// <summary>
    /// To count the number of non-folder resources in the collection.
    /// created.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This is different from childcount in that it omits counting child
    /// collections for which "isfolder" is true.
    /// </para>
    /// <para>
    /// This is an extend WebDAV collection property as defined in the
    /// <see href="https://tools.ietf.org/html/draft-hopmann-collection-props-00#section-1.2">
    /// draft document of Alex Hopmann (Microsoft) and Lisa Lippert (Microsoft)
    /// </see>.
    /// </para>
    /// </remarks>
    /// <typeparam name="TEntry">
    /// Store item or collection to which this DAV property applies.
    /// </typeparam>
    public class DavExtCollectionObjectCount<TEntry> : DavInt32<TEntry> where TEntry : IStoreItem
    {
        /// <summary>
        /// Name of the property (static).
        /// </summary>
        // ReSharper disable once StaticMemberInGenericType
        public static readonly XName PropertyName = WebDavNamespaces.DavNs + "objectcount";

        /// <summary>
        /// Name of the property.
        /// </summary>
        public override XName Name => PropertyName;
    }

    /// <summary>
    /// Specifies whether or not the collection is reserved.
    /// </summary>
    /// <remarks>
    /// <para>
    /// A reserved collection is one that is specially managed by the server
    /// and cannot be deleted, renamed, or moved by the client. Attempts to
    /// MOVE or DELETE a reserved collection will fail, and this SHOULD be
    /// reflected in the client UI. If absent, the collection should NOT be
    /// reserved. The server may allow clients to set this property. It may
    /// make sense to also specify that this collection is reserved in the
    /// resourcetype; however, in most ways this behaves like a normal
    /// collection.
    /// </para>
    /// <para>
    /// This is an extend WebDAV collection property as defined in the
    /// <see href="https://tools.ietf.org/html/draft-hopmann-collection-props-00#section-1.2">
    /// draft document of Alex Hopmann (Microsoft) and Lisa Lippert (Microsoft)
    /// </see>.
    /// </para>
    /// </remarks>
    /// <typeparam name="TEntry">
    /// Store item or collection to which this DAV property applies.
    /// </typeparam>
    public class DavExtCollectionReserved<TEntry> : DavBoolean<TEntry> where TEntry : IStoreItem
    {
        /// <summary>
        /// Name of the property (static).
        /// </summary>
        // ReSharper disable once StaticMemberInGenericType
        public static readonly XName PropertyName = WebDavNamespaces.DavNs + "reserved";

        /// <summary>
        /// Name of the property.
        /// </summary>
        public override XName Name => PropertyName;
    }

    /// <summary>
    /// Counts the number of visible non-folder resources in the collection.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This is the most immediately useful property for the client UI to use
    /// to display the sizes of collections for users. The client UI could
    /// also display progess when downloading a long list of children in a
    /// collection if it knows the total number in advance. This counts all
    /// children for which "ishidden" is false and "isfolder" is false.
    /// </para>
    /// <para>
    /// This is an extend WebDAV collection property as defined in the
    /// <see href="https://tools.ietf.org/html/draft-hopmann-collection-props-00#section-1.2">
    /// draft document of Alex Hopmann (Microsoft) and Lisa Lippert (Microsoft)
    /// </see>.
    /// </para>
    /// </remarks>
    /// <typeparam name="TEntry">
    /// Store item or collection to which this DAV property applies.
    /// </typeparam>
    public class DavExtCollectionVisibleCount<TEntry> : DavInt32<TEntry> where TEntry : IStoreItem
    {
        /// <summary>
        /// Name of the property (static).
        /// </summary>
        // ReSharper disable once StaticMemberInGenericType
        public static readonly XName PropertyName = WebDavNamespaces.DavNs + "visiblecount";

        /// <summary>
        /// Name of the property.
        /// </summary>
        public override XName Name => PropertyName;
    }
}
