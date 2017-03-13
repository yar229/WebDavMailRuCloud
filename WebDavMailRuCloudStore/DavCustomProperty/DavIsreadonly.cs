using System.Xml.Linq;
using NWebDav.Server;
using NWebDav.Server.Props;
using NWebDav.Server.Stores;

namespace YaR.WebDavMailRu.CloudStore.DavCustomProperty
{
    public class DavIsreadonly<TEntry> : DavBoolean<TEntry> where TEntry : IStoreItem
    {
        /// <summary>
        /// Name of the property (static).
        /// </summary>
        // ReSharper disable once StaticMemberInGenericType
        public static readonly XName PropertyName = WebDavNamespaces.DavNs + "isreadonly";

        /// <summary>
        /// Name of the property.
        /// </summary>
        public override XName Name => PropertyName;
    }
}