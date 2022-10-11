using System.Xml.Linq;

using NWebDav.Server.Helpers;

namespace NWebDav.Server.Locking
{
    public readonly struct LockEntry
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
            return new XElement(WebDavNamespaces.DavNsLockEntry,
                new XElement(WebDavNamespaces.DavNsLockScope, new XElement(WebDavNamespaces.DavNs + XmlHelper.GetXmlValue(Scope))),
                new XElement(WebDavNamespaces.DavNsLockType, new XElement(WebDavNamespaces.DavNs + XmlHelper.GetXmlValue(Type))));
        }
    }
}