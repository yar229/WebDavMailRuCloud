using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Security.Policy;


namespace YaR.Clouds.Base
{
    public class PublicLinkInfo
    {
        public PublicLinkInfo(string type, string baseurl, string urlstring)//:this(type, new Uri(baseurl) + new Uri(urlstring)))
        {
            Init(type, baseurl, urlstring);
        }

        public PublicLinkInfo(string type, string urlstring):this(type, new Uri(urlstring))
        {
        }

        public PublicLinkInfo(string urlstring):this("", new Uri(urlstring))
        {
        }
        
        public PublicLinkInfo(string type, Uri url)
        {
            Init(type,url);
        }

        public PublicLinkInfo(Uri url):this("", url)
        {
        }       

        private void Init(string type, string baseurl, string urlstring)
        {
            var u = UrlPathCombine(baseurl, urlstring);
            Init(type, u);

        }

        private void Init(string type, Uri url)
        {
            if (!url.IsAbsoluteUri)
                throw new ArgumentException("Absolute uri required");

            Type = type;
            _uri = url;
        }
        
        public string Type { get; set; }

        public Uri Uri
        {
            get => _uri;
            private set
            {
                _uri = value;
            }
        }
        private Uri _uri;


        private static Uri UrlPathCombine(string path1, string path2)
        {
            path1 = path1.TrimEnd('/') + "/";
            path2 = path2.TrimStart('/');

            return new Uri(path1 + path2, UriKind.Absolute);

        }
    }
}
