using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using NWebDav.Server;
using NWebDav.Server.Props;
using NWebDav.Server.Stores;

namespace YaR.WebDavMailRu.CloudStore
{
    public class DavExtCollectionQuotaAvailableBytes<TEntry> : DavInt64<TEntry> where TEntry : IStoreItem
    {
        public static readonly XName PropertyName = WebDavNamespaces.DavNs + "quota-available-bytes";

        public override XName Name
        {
            get
            {
                return DavExtCollectionQuotaAvailableBytes<TEntry>.PropertyName;
            }
        }
    }
}
