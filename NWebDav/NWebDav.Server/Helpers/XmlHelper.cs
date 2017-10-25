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

            var xmlEnumAttribute = memberInfo.GetCustomAttribute<XmlEnumAttribute>();
            return xmlEnumAttribute?.Name;
        }
    }
}
