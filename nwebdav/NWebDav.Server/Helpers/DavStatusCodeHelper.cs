using System;
using System.Linq;
using System.Reflection;

namespace NWebDav.Server.Helpers
{
    public static class DavStatusCodeHelper
    {
        public static string GetStatusDescription<TEnum>(TEnum value, string defaultValue = null) where TEnum : struct
        {
            // Obtain the member information
            var memberInfo = typeof(TEnum).GetMember(value.ToString()).FirstOrDefault();
            if (memberInfo == null)
                return defaultValue;

            var davStatusCodeAttribute = memberInfo.GetCustomAttribute<DavStatusCodeAttribute>();
            return davStatusCodeAttribute?.Description;
        }
    }
}
