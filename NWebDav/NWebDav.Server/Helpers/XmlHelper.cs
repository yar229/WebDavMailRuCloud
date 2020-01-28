using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Xml.Serialization;

namespace NWebDav.Server.Helpers
{
    public static class XmlHelper
    {
        public static string GetXmlValue<TEnum>(TEnum value, string defaultValue = null) where TEnum : struct, IComparable, IFormattable, IConvertible
        {
            return EnumNameCache<TEnum>.GetName(value);
        }

        //// YaR: optimize 
        internal static class EnumNameCache<T> where T : struct, IComparable, IFormattable, IConvertible
        {
            private static readonly Dictionary<T, string> NameMap;

            static EnumNameCache()
            {
                NameMap = new Dictionary<T, string>();
                Type type = typeof(T);
                foreach (T value in Enum.GetValues(type).Cast<T>())
                {
                    string valueName = value.ToString();
                    NameMap.Add(value, type.GetMember(valueName)[0].GetCustomAttribute<XmlEnumAttribute>()?.Name ?? valueName);
                }
            }

            public static string GetName(T value)
            {
                return NameMap[value];
            }
        }
    }
}
