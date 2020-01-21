using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Xml.Serialization;

namespace NWebDav.Server.Helpers
{
    public static class XmlHelper
    {
        public static string GetXmlValue<TEnum>(TEnum value, string defaultValue = null) where TEnum : struct
        {
            // Obtain the member information
            var memberInfo = typeof(TEnum).GetMember(value.ToString()).FirstOrDefault();
            if (memberInfo == null)
                return defaultValue;

            // YaR: optimize 
            if (EnumCache.TryGetValue(memberInfo, out string cachedName))
                return cachedName;

            var xmlEnumAttribute = memberInfo.GetCustomAttribute<XmlEnumAttribute>();
            if (xmlEnumAttribute != null)
                EnumCache[memberInfo] = xmlEnumAttribute.Name;
            return xmlEnumAttribute?.Name;
        }

        // YaR: optimize 
        private static readonly Dictionary<MemberInfo, string> EnumCache = new Dictionary<MemberInfo, string>();
    }
}
