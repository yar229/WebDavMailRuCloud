using NWebDav.Server;
using NWebDav.Server.Props;
using NWebDav.Server.Stores;
using System.Xml.Linq;

namespace YaR.WebDavMailRu.CloudStore.DavCustomProperty
{
    public class DavSharedLink<TEntry> : DavString<TEntry> where TEntry : IStoreItem
    {
        public static readonly XName PropertyName = WebDavNamespaces.DavNs + "SharedLink";

        public override XName Name
        {
            get
            {
                return DavSharedLink<TEntry>.PropertyName;
            }
        }
    }

}
