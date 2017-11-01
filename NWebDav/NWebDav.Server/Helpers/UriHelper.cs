using System;
using System.Linq;

namespace NWebDav.Server.Helpers
{
    public static class UriHelper
    {
        public static WebDavUri Combine(WebDavUri baseUri, string path)
        {
            var uriText = baseUri.OriginalString;
            if (uriText.EndsWith("/"))
                uriText = uriText.Substring(0, uriText.Length - 1);
            return new WebDavUri($"{uriText}/{Uri.EscapeDataString(path)}");
        }

        //public static string ToEncodedString(Uri entryUri)
        //{
        //    return entryUri
        //        .AbsoluteUri
        //        .Replace("#", "%23")
        //        .Replace("[", "%5B")
        //        .Replace("]", "%5D");
        //}

        public static string GetDecodedPath(Uri uri)
        {
            return uri.LocalPath + Uri.UnescapeDataString(uri.Fragment);
        }
    }
}
