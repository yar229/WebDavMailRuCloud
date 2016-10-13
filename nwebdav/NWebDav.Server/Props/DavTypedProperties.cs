using System;
using System.Collections.Generic;
using System.Globalization;
using System.Xml;
using System.Xml.Linq;

using NWebDav.Server.Http;
using NWebDav.Server.Stores;

namespace NWebDav.Server.Props
{
    public abstract class DavTypedProperty<TEntry, TType> : DavProperty<TEntry> where TEntry : IStoreItem
    {
        /// <summary>
        /// Converter defining methods to convert property values from/to XML.
        /// </summary>
        public interface IConverter
        {
            /// <summary>
            /// Get the XML representation of the specified value.
            /// </summary>
            /// <param name="httpContext">
            /// Current HTTP context.
            /// </param>
            /// <param name="value">
            /// Value that needs to be converted to XML output.
            /// </param>
            /// <returns>
            /// The XML representation of the <see cref="value"/>. The XML
            /// output should either be a <see cref="System.String"/> or an 
            /// <see cref="System.Xml.Linq.XElement"/>.
            /// </returns>
            /// <remarks>
            /// The current HTTP context can be used to generate XML that is
            /// compatible with the requesting WebDAV client.
            /// </remarks>
            object ToXml(IHttpContext httpContext, TType value);
            
            /// <summary>
            /// Get the typed value of the specified XML representation.
            /// </summary>
            /// <param name="httpContext">
            /// Current HTTP context.
            /// </param>
            /// <param name="value">
            /// The XML value that needs to be converted to the target
            /// type. This value is always a <see cref="System.String"/>
            /// or an <see cref="System.Xml.Linq.XElement"/>.
            /// </param>
            /// <returns>
            /// The typed value of the XML representation.
            /// </returns>
            /// <remarks>
            /// The current HTTP context can be used to generate XML that is
            /// compatible with the requesting WebDAV client.
            /// </remarks>
            TType FromXml(IHttpContext httpContext, object value);
        }

        private Func<IHttpContext, TEntry, TType> _getter;
        private Func<IHttpContext, TEntry, TType, DavStatusCode> _setter;

        public new Func<IHttpContext, TEntry, TType> Getter
        {
            get { return _getter; }
            set
            {
                _getter = value;
                base.Getter = (c, s) =>
                {
                    var v = _getter(c, s);
                    return Converter != null ? Converter.ToXml(c, v) : v;
                };
            }
        }

        public new Func<IHttpContext, TEntry, TType, DavStatusCode> Setter
        {
            get { return _setter; }
            set
            {
                _setter = value;
                base.Setter = (c, s, v) =>
                {
                    var tv = Converter != null ? Converter.FromXml(c, v) : (TType)v;
                    return _setter(c, s, tv);
                };
            }
        }

        public abstract IConverter Converter { get; }
    }

    public abstract class DavRfc1123Date<TEntry> : DavTypedProperty<TEntry, DateTime> where TEntry : IStoreItem
    {
        private class Rfc1123DateConverter : IConverter
        {
            public object ToXml(IHttpContext httpContext, DateTime value) => value.ToString("R");
            public DateTime FromXml(IHttpContext httpContext, object value) => DateTime.Parse((string)value);
        }

        private static IConverter TypeConverter { get; } = new Rfc1123DateConverter();

        public override IConverter Converter => TypeConverter;
    }

    public abstract class DavIso8601Date<TEntry> : DavTypedProperty<TEntry, DateTime> where TEntry : IStoreItem
    {
        private class Iso8601DateConverter : IConverter
        {
            public object ToXml(IHttpContext httpContext, DateTime value)
            {
                // The older built-in Windows WebDAV clients have a problem, so
                // they cannot deal with more than 3 digits for the
                // milliseconds.
                if (HasIso8601FractionBug(httpContext))
                {
                    // We need to recreate the date again, because the Windows 7
                    // WebDAV client cannot 
                    var dt = new DateTime(value.Year, value.Month, value.Day, value.Hour, value.Minute, value.Second, value.Millisecond, DateTimeKind.Utc);
                    return XmlConvert.ToString(dt, XmlDateTimeSerializationMode.Utc);
                }

                return XmlConvert.ToString(value, XmlDateTimeSerializationMode.Utc);
            }

            public DateTime FromXml(IHttpContext httpContext, object value) => XmlConvert.ToDateTime((string)value, XmlDateTimeSerializationMode.Utc);

            private bool HasIso8601FractionBug(IHttpContext httpContext)
            {
                // TODO: Determine which WebDAV clients have this bug
                return true;
            }
        }

        private static IConverter TypeConverter { get; } = new Iso8601DateConverter();

        public override IConverter Converter => TypeConverter;
    }

    public abstract class DavBoolean<TEntry> : DavTypedProperty<TEntry, Boolean> where TEntry : IStoreItem
    {
        private class BooleanConverter : IConverter
        {
            public object ToXml(IHttpContext httpContext, Boolean value) => value ? "1" : "0";
            public Boolean FromXml(IHttpContext httpContext, object value) => int.Parse(value.ToString()) != 0;
        }

        private static IConverter TypeConverter { get; } = new BooleanConverter();

        public override IConverter Converter => TypeConverter;
    }

    public abstract class DavString<TEntry> : DavTypedProperty<TEntry, string> where TEntry : IStoreItem
    {
        private class StringConverter : IConverter
        {
            public object ToXml(IHttpContext httpContext, string value) => value;
            public string FromXml(IHttpContext httpContext, object value) => value.ToString();
        }

        private static IConverter TypeConverter { get; } = new StringConverter();

        public override IConverter Converter => TypeConverter;
    }

    public abstract class DavInt32<TEntry> : DavTypedProperty<TEntry, Int32> where TEntry : IStoreItem
    {
        private class Int32Converter : IConverter
        {
            public object ToXml(IHttpContext httpContext, Int32 value) => value.ToString(CultureInfo.InvariantCulture);
            public Int32 FromXml(IHttpContext httpContext, object value) => int.Parse(value.ToString(), NumberStyles.Integer, CultureInfo.InvariantCulture);
        }

        private static IConverter TypeConverter { get; } = new Int32Converter();

        public override IConverter Converter => TypeConverter;
    }

    public abstract class DavInt64<TEntry> : DavTypedProperty<TEntry, Int64> where TEntry : IStoreItem
    {
        private class Int64Converter : IConverter
        {
            public object ToXml(IHttpContext httpContext, Int64 value) => value.ToString(CultureInfo.InvariantCulture);
            public Int64 FromXml(IHttpContext httpContext, object value) => int.Parse(value.ToString(), NumberStyles.Integer, CultureInfo.InvariantCulture);
        }

        private static IConverter TypeConverter { get; } = new Int64Converter();

        public override IConverter Converter => TypeConverter;
    }

    public abstract class DavXElementArray<TEntry> : DavTypedProperty<TEntry, IEnumerable<XElement>> where TEntry : IStoreItem
    {
        private class XElementArrayConverter : IConverter
        {
            public object ToXml(IHttpContext httpContext, IEnumerable<XElement> value) => value;
            public IEnumerable<XElement> FromXml(IHttpContext httpContext, object value) => (IEnumerable<XElement>)value;
        }

        private static IConverter TypeConverter { get; } = new XElementArrayConverter();

        public override IConverter Converter => TypeConverter;
    }

    public abstract class DavXElement<TEntry> : DavTypedProperty<TEntry, XElement> where TEntry : IStoreItem
    {
        private class XElementConverter : IConverter
        {
            public object ToXml(IHttpContext httpContext, XElement value) => value;
            public XElement FromXml(IHttpContext httpContext, object value) => (XElement)value;
        }

        private static IConverter TypeConverter { get; } = new XElementConverter();

        public override IConverter Converter => TypeConverter;
    }

    public abstract class DavUri<TEntry> : DavTypedProperty<TEntry, Uri> where TEntry : IStoreItem
    {
        private class UriConverter : IConverter
        {
            public object ToXml(IHttpContext httpContext, Uri value) => value.ToString();
            public Uri FromXml(IHttpContext httpContext, object value) => new Uri((string)value);
        }

        private static IConverter TypeConverter { get; } = new UriConverter();

        public override IConverter Converter => TypeConverter;
    }

}
