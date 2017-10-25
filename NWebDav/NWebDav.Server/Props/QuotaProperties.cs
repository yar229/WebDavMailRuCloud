using System.Xml.Linq;

using NWebDav.Server.Stores;

namespace NWebDav.Server.Props
{
    /// <summary>
    /// Indicates the maximum amount of additional storage available to be
    /// allocated to a resource.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The <c>DAV:quota-available-bytes</c> property value is the value in
    /// octets representing the amount of additional disk space beyond the
    /// current allocation that can be allocated to this resource before
    /// further allocations will be refused.It is understood that this space
    /// may be consumed by allocations to other resources.
    /// </para>
    /// <para>
    /// Support for this property is REQUIRED on collections, and OPTIONAL on
    /// other resources.A server SHOULD implement this property for each
    /// resource that has the <c>DAV:quota-used-bytes</c> property.
    /// </para>
    /// <para>
    /// Clients SHOULD expect that as the <c>DAV:quota-available-bytes</c> on
    /// a resource approaches 0, further allocations to that resource may be
    /// refused. A value of 0 indicates that users will probably not be able
    /// to perform operations that write additional information (e.g., a PUT
    /// inside a collection), but may be able to replace through overwrite an
    /// existing resource of equal size.
    /// </para>
    /// <para>
    /// Note that there may be a number of distinct but overlapping limits,
    /// which may even include physical media limits. When reporting
    /// <c>DAV:quota-available-bytes</c>, the server is at liberty to choose
    /// any of those limits but SHOULD do so in a repeatable way. The rule may
    /// be configured per repository, or may be "choose the smallest number".
    /// </para>
    /// <para>
    /// If a resource has no quota enforced or unlimited storage ("infinite
    /// limits"), the server MAY choose not to return this property (404 Not
    /// Found response in Multi-Status), although this specification
    /// RECOMMENDS that servers return some appropriate value (e.g., the
    /// amount of free disk space). A client cannot entirely assume that
    /// there is no quota enforced on a resource that does not have this
    /// property, but might as well act as if there is no quota.
    /// </para>
    /// <para>
    /// The value of this property is protected (see Section 1.4.2 of
    /// [RFC3253] for the definition of protected properties). A 403
    /// Forbidden response is RECOMMENDED for attempts to write a protected
    /// property, and the server SHOULD include an XML error body as defined
    /// by DeltaV[RFC3253] with the 
    /// <c>&lt;DAV:cannot-modify-protected-property/&gt;</c>
    /// precondition tag.
    /// </para>
    /// </remarks>
    /// <typeparam name="TEntry">
    /// Store item or collection to which this DAV property applies.
    /// </typeparam>
    public class DavQuotaAvailableBytes<TEntry> : DavInt64<TEntry> where TEntry : IStoreItem
    {
        /// <summary>
        /// Name of the property (static).
        /// </summary>
        // ReSharper disable once StaticMemberInGenericType
        public static readonly XName PropertyName = WebDavNamespaces.DavNs + "quota-available-bytes";

        /// <summary>
        /// Name of the property.
        /// </summary>
        public override XName Name => PropertyName;
    }

    /// <summary>
    /// Contains the amount of storage counted against the quota on a resource.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The <c>DAV:quota-used-bytes</c> value is the value in octets
    /// representing the amount of space used by this resource and possibly a
    /// number of other similar resources, where the set of "similar" meets
    /// at least the criterion that allocating space to any resource in the
    /// set will count against the <c>DAV:quota-available-bytes</c>. It MUST
    /// include the total count including usage derived from sub-resources if
    /// appropriate. It SHOULD include metadata storage size if metadata
    /// storage is counted against the DAV:quota-available-bytes.
    /// </para>
    /// <para>
    /// SNote that there may be a number of distinct but overlapping sets of
    /// resources for which a <c>DAV:quota-used-bytes</c> is maintained
    /// (e.g., "all files with a given owner", "all files with a given group
    /// owner", etc.). The server is at liberty to choose any of those sets
    /// but SHOULD do so in a repeatable way. The rule may be configured per
    /// repository.
    /// </para>
    /// <para>
    /// Support for this property is REQUIRED on collections, and OPTIONAL on
    /// other resources. A server SHOULD implement this property for each
    /// resource that has the DAV:quota-available-bytes property.
    /// </para>
    /// <para>
    /// This value of this property is computed (see Section 1.4.3 of
    /// [RFC3253] for the definition of computed property). A 403
    /// Forbidden response is RECOMMENDED for attempts to write a protected
    /// property, and the server SHOULD include an XML error body as defined
    /// by DeltaV [RFC3253] with the 
    /// <c>&lt;DAV:cannot-modify-protected-property/&gt;</c>
    /// precondition tag.
    /// </para>
    /// </remarks>
    /// <typeparam name="TEntry">
    /// Store item or collection to which this DAV property applies.
    /// </typeparam>
    public class DavQuotaUsedBytes<TEntry> : DavInt64<TEntry> where TEntry : IStoreItem
    {
        /// <summary>
        /// Name of the property (static).
        /// </summary>
        // ReSharper disable once StaticMemberInGenericType
        public static readonly XName PropertyName = WebDavNamespaces.DavNs + "quota-used-bytes";

        /// <summary>
        /// Name of the property.
        /// </summary>
        public override XName Name => PropertyName;
    }
}
