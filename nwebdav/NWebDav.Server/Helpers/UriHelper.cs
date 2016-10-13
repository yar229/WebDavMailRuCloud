using System;

namespace NWebDav.Server.Helpers
{
    public static class UriHelper
    {
        public static Uri Combine(Uri baseUri, string path)
        {
            var uriText = baseUri.OriginalString;
            if (uriText.EndsWith("/"))
                return new Uri(baseUri, path);
            return new Uri($"{uriText}/{path}", UriKind.Absolute);
        }
    }
}
