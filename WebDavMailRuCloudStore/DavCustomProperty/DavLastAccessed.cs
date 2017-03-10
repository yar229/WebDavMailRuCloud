using System.Xml.Linq;
using NWebDav.Server;
using NWebDav.Server.Props;
using NWebDav.Server.Stores;

namespace YaR.WebDavMailRu.CloudStore.DavCustomProperty
{
    public class DavLastAccessed<TEntry> : DavRfc1123Date<TEntry> where TEntry : IStoreItem
    {
        /// <summary>Name of the property (static).</summary>
        public static readonly XName PropertyName = WebDavNamespaces.DavNs + "lastaccessed";

        /// <summary>Name of the property.</summary>
        public override XName Name => DavLastAccessed<TEntry>.PropertyName;
    }

}