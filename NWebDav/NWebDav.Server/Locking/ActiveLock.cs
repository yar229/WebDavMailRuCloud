using System;
using System.Globalization;
using System.Xml.Linq;

using NWebDav.Server.Helpers;

namespace NWebDav.Server.Locking
{
    public struct ActiveLock
    {
        public LockType Type { get; }
        public LockScope Scope { get; }
        public int Depth { get; }
        public XElement Owner { get; }
        public int Timeout { get; }
        public Uri LockToken { get; }
        public Uri LockRoot { get; }

        public ActiveLock(LockType type, LockScope scope, int depth, XElement owner, int timeout, Uri lockToken, Uri lockRoot)
        {
            Type = type;
            Scope = scope;
            Depth = depth;
            Owner = owner;
            Timeout = timeout;
            LockToken = lockToken;
            LockRoot = lockRoot;
        }

        public XElement ToXml()
        {
            return new XElement(WebDavNamespaces.DavNs + "activelock",
                new XElement(WebDavNamespaces.DavNs + "locktype", new XElement(WebDavNamespaces.DavNs + XmlHelper.GetXmlValue(Type))),
                new XElement(WebDavNamespaces.DavNs + "lockscope", new XElement(WebDavNamespaces.DavNs + XmlHelper.GetXmlValue(Scope))),
                new XElement(WebDavNamespaces.DavNs + "depth", Depth == int.MaxValue ? "infinity" : Depth.ToString(CultureInfo.InvariantCulture)),
                new XElement(WebDavNamespaces.DavNs + "owner", Owner),
                new XElement(WebDavNamespaces.DavNs + "timeout", Timeout == -1 ? "Infinite" : "Second-" + Timeout.ToString(CultureInfo.InvariantCulture)),
                new XElement(WebDavNamespaces.DavNs + "locktoken", new XElement(WebDavNamespaces.DavNs + "href", LockToken.AbsoluteUri)),
                new XElement(WebDavNamespaces.DavNs + "lockroot", new XElement(WebDavNamespaces.DavNs + "href", LockRoot.AbsoluteUri)));
        }
    }
}
