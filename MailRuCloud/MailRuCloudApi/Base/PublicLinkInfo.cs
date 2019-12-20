using System;


namespace YaR.Clouds.Base
{
    public struct PublicLinkInfo
    {
        public PublicLinkInfo(string type, string urlstring):this(type, new Uri(urlstring))
        {
        }

        public PublicLinkInfo(string urlstring):this("", new Uri(urlstring))
        {
        }
        
        public PublicLinkInfo(string type, Uri url)
        {
            if (!url.IsAbsoluteUri)
                throw new ArgumentException("Absolute uri required");

            Type = type;
            _uri = url;
        }       
        
        public PublicLinkInfo(Uri url):this("", url)
        {
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
    }
}
