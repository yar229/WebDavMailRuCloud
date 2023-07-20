using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace YaR.Clouds.WebDavStore
{
    public class BrowserAuthenticatorInfo
    {
        public string Url { get; private set; }

        public string Password { get; private set; }

        public string CacheDir { get; private set; }

        public BrowserAuthenticatorInfo(string url, string password, string cacheDir )
        {
            Url = url;
            Password = password;
            CacheDir = cacheDir;
        }
    }

    public static class BrowserAuthenticator
    {
        public static BrowserAuthenticatorInfo Instance { get; set; }
    }
}
