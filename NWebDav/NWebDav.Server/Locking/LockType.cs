using System.Xml.Serialization;

namespace NWebDav.Server.Locking
{
    public enum LockType
    {
        [XmlEnum("write")]
        Write
    }
}
