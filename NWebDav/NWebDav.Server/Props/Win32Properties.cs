using System;
using System.IO;
using System.Xml.Linq;

using NWebDav.Server.Http;
using NWebDav.Server.Stores;

namespace NWebDav.Server.Props
{
    /// <summary>
    /// Contains the creation time of the collection or item.
    /// </summary>
    /// <remarks>
    /// Note that this property returns the date in HTTP format, which is
    /// different from the standard XML representation as in ISO 8601.
    /// </remarks>
    /// <typeparam name="TEntry">
    /// Store item or collection to which this DAV property applies.
    /// </typeparam>
    public class Win32CreationTime<TEntry> : DavRfc1123Date<TEntry> where TEntry : IStoreItem
    {
        /// <summary>
        /// Name of the property (static).
        /// </summary>
        // ReSharper disable once StaticMemberInGenericType
        public static readonly XName PropertyName = WebDavNamespaces.Win32Ns + "Win32CreationTime";

        /// <summary>
        /// Name of the property.
        /// </summary>
        public override XName Name => PropertyName;
    }

    /// <summary>
    /// Contains the time that the collection or item was accessed for the last
    /// time.
    /// </summary>
    /// <remarks>
    /// Note that this property returns the date in HTTP format, which is
    /// different from the standard XML representation as in ISO 8601.
    /// </remarks>
    /// <typeparam name="TEntry">
    /// Store item or collection to which this DAV property applies.
    /// </typeparam>
    public class Win32LastAccessTime<TEntry> : DavRfc1123Date<TEntry> where TEntry : IStoreItem
    {
        /// <summary>
        /// Name of the property (static).
        /// </summary>
        // ReSharper disable once StaticMemberInGenericType
        public static readonly XName PropertyName = WebDavNamespaces.Win32Ns + "Win32LastAccessTime";

        /// <summary>
        /// Name of the property.
        /// </summary>
        public override XName Name => PropertyName;
    }

    /// <summary>
    /// Contains the time that the collection or item was modfied for the last
    /// time.
    /// </summary>
    /// <remarks>
    /// Note that this property returns the date in HTTP format, which is
    /// different from the standard XML representation as in ISO 8601.
    /// </remarks>
    /// <typeparam name="TEntry">
    /// Store item or collection to which this DAV property applies.
    /// </typeparam>
    public class Win32LastModifiedTime<TEntry> : DavRfc1123Date<TEntry> where TEntry : IStoreItem
    {
        /// <summary>
        /// Name of the property (static).
        /// </summary>
        // ReSharper disable once StaticMemberInGenericType
        public static readonly XName PropertyName = WebDavNamespaces.Win32Ns + "Win32LastModifiedTime";

        /// <summary>
        /// Name of the property.
        /// </summary>
        public override XName Name => PropertyName;
    }

    /// <summary>
    /// Contains the hexadecimal representation of the collection or item's
    /// file attributes.
    /// time.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The file attributes are returned as an 8-character hexadecimal number
    /// representing a 32-bit value, where each bit has a specific meaning.
    /// </para>
    /// <list type="table">
    /// <listheader>
    /// <term>Bit</term>
    /// <term>Value</term>
    /// <term>Description</term>
    /// </listheader>
    /// <item><term>0</term><term>1</term><term>ReadOnly</term></item>
    /// <item><term>1</term><term>2</term><term>Hidden</term></item>
    /// <item><term>2</term><term>4</term><term>System</term></item>
    /// <item><term>4</term><term>16</term><term>Directory</term></item>
    /// <item><term>5</term><term>32</term><term>Archive</term></item>
    /// <item><term>6</term><term>64</term><term>Device</term></item>
    /// <item><term>7</term><term>128</term><term>Normal</term></item>
    /// <item><term>8</term><term>256</term><term>Temporary</term></item>
    /// <item><term>9</term><term>512</term><term>SparseFile</term></item>
    /// <item><term>10</term><term>1024</term><term>ReparsePoint</term></item>
    /// <item><term>11</term><term>2048</term><term>Compressed</term></item>
    /// <item><term>12</term><term>4096</term><term>Offline</term></item>
    /// <item><term>13</term><term>8192</term><term>NotContentIndexed</term></item>
    /// <item><term>14</term><term>16384</term><term>Encrypted</term></item>
    /// <item><term>15</term><term>32768</term><term>IntegrityStream</term></item>
    /// <item><term>17</term><term>131072</term><term>NoScrubData</term></item>
    /// </list>
    /// <para>
    /// When programming on the Windows platform the integer representation of
    /// <see cref="System.IO.FileAttributes"/> can be used.
    /// </para>
    /// </remarks>
    /// <typeparam name="TEntry">
    /// Store item or collection to which this DAV property applies.
    /// </typeparam>
    public class Win32FileAttributes<TEntry> : DavTypedProperty<TEntry, FileAttributes> where TEntry : IStoreItem
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
        public static readonly XName PropertyName = WebDavNamespaces.Win32Ns + "Win32FileAttributes";

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
