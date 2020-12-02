using System;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;

namespace YaR.Clouds.Extensions
{
    public static class EnumExtensions
    {
        public static T ParseEnumMemberValue<T>(string stringValue, bool ignoreCase = true)
            where T: Enum
        {
            T output = default;
            string enumStringValue = null;

            var type = typeof(T);
            foreach (FieldInfo fi in type.GetFields())
            {
                if (fi.GetCustomAttributes(typeof (EnumMemberAttribute), false) is EnumMemberAttribute[] attrs && attrs.Length > 0)
                    enumStringValue = attrs[0].Value;

                if (string.Compare(enumStringValue, stringValue, ignoreCase) == 0)
                {
                    output = (T)Enum.Parse(type, fi.Name);
                    break;
                }
            }

            return output;
        }

        public static string ToEnumMemberValue(this Enum @enum)
        {
            var attr =  @enum.GetType()
                .GetMember(@enum.ToString()).FirstOrDefault()?
                .GetCustomAttributes(false)
                .OfType<EnumMemberAttribute>().
                FirstOrDefault();

            return attr == null 
                ? @enum.ToString() 
                : attr.Value;
        }
    }
}
