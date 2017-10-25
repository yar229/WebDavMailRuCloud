using System.Xml.Linq;

using NWebDav.Server.Helpers;

namespace NWebDav.Server.Locking
{
    public struct LockEntry
    {
        public LockScope Scope { get; }
        public LockType Type { get; }

        public LockEntry(LockScope scope, LockType type)
        {
            Scope = scope;
            Type = type;
        }

        public XElement ToXml()
        {
            return new XElement(WebDavNamespaces.DavNs + "lockentry",
                new XElement(WebDavNamespaces.DavNs + "lockscope", new XElement(WebDavNamespaces.DavNs + XmlHelper.GetXmlValue(Scope))),
                new XElement(WebDavNamespaces.DavNs + "locktype", new XElement(WebDavNamespaces.DavNs + XmlHelper.GetXmlValue(Type))));
        }
    }
}