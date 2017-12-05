using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace NWebDav.Server
{
    public class WebDavUri
    {
        private readonly string _url;
        private readonly Uri _fakeurl;


        public WebDavUri(string url)
        {
            _url = url;
            _fakeurl = new Uri(_url);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="baseUrl"></param>
        /// <param name="relaUrl">encoded webdav path</param>
        public WebDavUri(string baseUrl, string relaUrl)
        {
            _url = baseUrl + relaUrl;
            _fakeurl = new Uri(_url);
        }

        public WebDavUri(WebDavUri url, string relaUrl)
        {
            _url = url.AbsoluteUri + relaUrl;
            _fakeurl = new Uri(_url);
        }

        public string AbsoluteUri => _url;

        public string OriginalString => _url;

        public string Scheme => _fakeurl.Scheme;

        /// <summary>
        /// Decoded path (standart decode, may fail)
        /// </summary>
        public string LocalPath =>  _fakeurl.LocalPath;

        /// <summary>
        /// decoded path
        /// </summary>
        public string Path
        {
            get
            {
                var requestedPath = Regex.Replace(_url, @"^http?://.*?(/|\Z)", string.Empty);
                requestedPath = "/" + requestedPath.TrimEnd('/');

                //if (string.IsNullOrWhiteSpace(requestedPath)) requestedPath = "/";

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
                var requestedPath = Regex.Replace(_url, @"^http?://.*?(/|\Z)", string.Empty);
                requestedPath = "/" + requestedPath.TrimEnd('/');

                //if (string.IsNullOrWhiteSpace(requestedPath)) requestedPath = "/";

                return requestedPath;
            }
        }

        public string BaseUrl
        {
            get
            {
                string res = $"{_fakeurl.Scheme}://{_fakeurl.Authority}";
                return res;
            }
        }

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
            return _url;
        }
    }

    public class UriAndName
    {
        public WebDavUri Parent { get; set; }
        public string Name { get; set; }
    }

}
