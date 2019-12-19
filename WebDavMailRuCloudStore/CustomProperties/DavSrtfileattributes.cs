using System;
using System.IO;
using System.Xml.Linq;
using NWebDav.Server;
using NWebDav.Server.Http;
using NWebDav.Server.Props;
using NWebDav.Server.Stores;

namespace YaR.Clouds.WebDavStore.CustomProperties
{
    public class DavSrtfileattributes<TEntry> : DavTypedProperty<TEntry, FileAttributes> where TEntry : IStoreItem
    {
        private class FileAttributesConverter : IConverter
        {
            public object ToXml(IHttpContext httpContext, FileAttributes value) => ((int)value).ToString("X8");
            public FileAttributes FromXml(IHttpContext httpContext, object value) => (FileAttributes)Convert.ToInt32((string)value, 16);
        }

        private static IConverter TypeConverter { get; } = new FileAttributesConverter();

        /// <summary>
        /// Name of the property (static).
        /// </summary>
        // ReSharper disable once StaticMemberInGenericType
        public static readonly XName PropertyName = WebDavNamespaces.DavNs + "SRT_fileattributes";

        /// <summary>
        /// Name of the property.
        /// </summary>
        public override XName Name => PropertyName;

        /// <summary>
        /// Converter that is used to convert the actual value into the XML
        /// format that is required by WebDAV.
        /// </summary>
        public override IConverter Converter => TypeConverter;
    }
}