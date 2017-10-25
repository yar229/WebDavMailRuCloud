using System.Xml.Serialization;

namespace NWebDav.Server.Locking
{
    public enum LockScope
    {
        [XmlEnum("exclusive")]
        Exclusive,

        [XmlEnum("shared")]
        Shared
    }
}
