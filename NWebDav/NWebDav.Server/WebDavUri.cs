using System;

namespace NWebDav.Server
{
    public class WebDavUri
    {
        //private readonly Uri _fakeurl;


        public WebDavUri(string url)
        {
            AbsoluteUri = url;
            //_fakeurl = new Uri(AbsoluteUri);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="baseUrl"></param>
        /// <param name="relaUrl">encoded webdav path</param>
        public WebDavUri(string baseUrl, string relaUrl)
        {
            AbsoluteUri = baseUrl + relaUrl;
            //_fakeurl = new Uri(AbsoluteUri);
        }

        //public WebDavUri(WebDavUri url, string relaUrl)
        //{
        //    AbsoluteUri = url.AbsoluteUri + relaUrl;
        //    _fakeurl = new Uri(AbsoluteUri);
        //}

        public string AbsoluteUri { get; }

        public string OriginalString => AbsoluteUri;

        public string Scheme 
        {
            get
            {
                int pos = AbsoluteUri.IndexOf("://", StringComparison.OrdinalIgnoreCase);
                if (pos < 0)
                    return "http";

                return AbsoluteUri.Substring(0, pos);
            }
        }  //_fakeurl.Scheme;

        /// <summary>
        /// Decoded path (standart decode, may fail)
        /// </summary>
        public string LocalPath => Path; //_fakeurl.LocalPath;

        /// <summary>
        /// decoded path
        /// </summary>
        public string Path
        {
            get
            {
                //var requestedPath = Regex.Replace(_url, @"^http?://.*?(/|\Z)", string.Empty);
                int pos = IndexOfNth(AbsoluteUri, '/', 3);
                string requestedPath = pos > -1
                    ? AbsoluteUri.Substring(pos + 1)
                    : AbsoluteUri;

                requestedPath = "/" + requestedPath.TrimEnd('/');
                
                requestedPath = Uri.UnescapeDataString(requestedPath);

                return requestedPath;
            }
        }

        /// <summary>
        /// Encoded path
        /// </summary>
        public string PathEncoded
        {
            get
            {
                if (string.IsNullOrEmpty(_pathEncoded))
                {
                    //var requestedPath = Regex.Replace(_url, @"^http?://.*?(/|\Z)", string.Empty, RegexOptions.CultureInvariant | RegexOptions.Compiled);
                    int pos = IndexOfNth(AbsoluteUri, '/', 3);
                    _pathEncoded = pos > -1
                        ? AbsoluteUri.Substring(pos + 1)
                        : AbsoluteUri;

                    _pathEncoded = "/" + _pathEncoded.TrimEnd('/');
                }

                return _pathEncoded;
            }
        }
        private string _pathEncoded;

        public string BaseUrl
        {
            get
            {
                //string res = $"{Scheme}://{_fakeurl.Authority}";
                //return res;

                if (string.IsNullOrEmpty(_baseUrl))
                {
                    int pos = IndexOfNth(AbsoluteUri, '/', 3);
                    _baseUrl = pos > -1
                        ? AbsoluteUri.Substring(0, pos + 1)
                        : AbsoluteUri;
                }

                return _baseUrl;
            }
        }

        private string _baseUrl;

        public UriAndName Parent
        {
            get
            {
                var trimmedUri = AbsoluteUri;
                if (trimmedUri.EndsWith("/"))
                    trimmedUri = trimmedUri.TrimEnd('/');

                // cause we use >> as a sign for special command
                int cmdPos = trimmedUri.IndexOf("%3e%3e", StringComparison.Ordinal);

                int slashOffset = cmdPos > 0
                    ? trimmedUri.LastIndexOf("/", cmdPos, StringComparison.InvariantCultureIgnoreCase)
                    : trimmedUri.LastIndexOf('/');

                if (slashOffset == -1)
                    return null;

                // Separate name from path
                return new UriAndName
                {
                    Parent = new WebDavUri(trimmedUri.Substring(0, slashOffset)),
                    Name = Uri.UnescapeDataString(trimmedUri.Substring(slashOffset + 1))
                };
               
            }
        }

        public override string ToString()
        {
            return AbsoluteUri;
        }




        private static int IndexOfNth(string str, char c, int nth, int startPosition = 0)
        {
            int index = str.IndexOf(c, startPosition);
            if (index >= 0 && nth > 1)
            {
                return  IndexOfNth(str, c, nth - 1, index + 1);
            }

            return index;
        }
    }

    public class UriAndName
    {
        public WebDavUri Parent { get; set; }
        public string Name { get; set; }
    }

}
