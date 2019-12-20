using System.Xml.Linq;
using NWebDav.Server;
using NWebDav.Server.Props;
using NWebDav.Server.Stores;

namespace YaR.Clouds.WebDavStore.CustomProperties
{
    public class DavSharedLink<TEntry> : DavString<TEntry> where TEntry : IStoreItem
    {
        private static readonly XName PropertyName = WebDavNamespaces.DavNs + "SharedLink";

        public override XName Name => PropertyName;
    }

}