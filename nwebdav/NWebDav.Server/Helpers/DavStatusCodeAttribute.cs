using System;

namespace NWebDav.Server.Helpers
{
    [AttributeUsage(AttributeTargets.Field)]
    public class DavStatusCodeAttribute : Attribute
    {
        public string Description { get; }

        public DavStatusCodeAttribute(string description)
        {
            Description = description;
        }
    }
}
