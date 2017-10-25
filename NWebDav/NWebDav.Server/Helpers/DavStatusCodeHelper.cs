using System.Linq;
using System.Reflection;

namespace NWebDav.Server.Helpers
{
    /// <summary>
    /// Helper methods for the <see cref="DavStatusCode"/> enumeration.
    /// </summary>
    public static class DavStatusCodeHelper
    {
        /// <summary>
        /// Obtain the human-readable status description for the specified
        /// <see cref="DavStatusCode"/>.
        /// </summary>
        /// <param name="davStatusCode">
        /// Code for which the description should be obtained.
        /// </param>
        /// <returns>
        /// Human-readable representation of the WebDAV status code.
        /// </returns>
        public static string GetStatusDescription(this DavStatusCode davStatusCode)
        {
            // Obtain the member information
            var memberInfo = typeof(DavStatusCode).GetMember(davStatusCode.ToString()).FirstOrDefault();
            if (memberInfo == null)
                return null;

            var davStatusCodeAttribute = memberInfo.GetCustomAttribute<DavStatusCodeAttribute>();
            return davStatusCodeAttribute?.Description;
        }
    }
}
