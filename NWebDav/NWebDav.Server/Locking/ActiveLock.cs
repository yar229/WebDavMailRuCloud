using System;
using System.Globalization;
using System.Xml.Linq;

using NWebDav.Server.Helpers;

namespace NWebDav.Server.Locking
{
    public readonly struct ActiveLock
    {
        public LockType Type { get; }
        public LockScope Scope { get; }
        public int Depth { get; }
        public XElement Owner { get; }
        public int Timeout { get; }
        public WebDavUri LockToken { get; }
        public WebDavUri LockRoot { get; }

        public ActiveLock(LockType type, LockScope scope, int depth, XElement owner, int timeout, WebDavUri lockToken, WebDavUri lockRoot)
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
            return new XElement(WebDavNamespaces.DavNsActiveLock,
                new XElement(WebDavNamespaces.DavNsLockType, new XElement(WebDavNamespaces.DavNs + XmlHelper.GetXmlValue(Type))),
                new XElement(WebDavNamespaces.DavNsLockScope, new XElement(WebDavNamespaces.DavNs + XmlHelper.GetXmlValue(Scope))),
                new XElement(WebDavNamespaces.DavNsDepth, Depth == int.MaxValue ? "infinity" : Depth.ToString(CultureInfo.InvariantCulture)),
                new XElement(WebDavNamespaces.DavNsOwner, Owner),
                new XElement(WebDavNamespaces.DavNsTimeout, Timeout == -1 ? "Infinite" : "Second-" + Timeout.ToString(CultureInfo.InvariantCulture)),
                new XElement(WebDavNamespaces.DavNsLockToken, new XElement(WebDavNamespaces.DavNsHref, LockToken.AbsoluteUri)),
                new XElement(WebDavNamespaces.DavNsLockRoot, new XElement(WebDavNamespaces.DavNsHref, LockRoot.AbsoluteUri)));
        }
    }
}
