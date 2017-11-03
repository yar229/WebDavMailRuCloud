using System;
using System.ComponentModel;

namespace YaR.MailRuCloud.Api.Base
{
    public static class WebDavPath
    {
        public static string Combine(string a, string b)
        {
            a = Clean(a);
            b = Clean(b);
            a = a.Trim('/');
            b = b.TrimStart('/');
            string res = a + (string.IsNullOrEmpty(b) ? "" : "/" + b);
            if (!res.StartsWith("/")) res = "/" + res;
            return res;

        }

        public static string Clean(string path)
        {
            string res = path.Replace("\\", "/");
            if (res.Length > 1)                
                return res.TrimEnd('/');
            return res;
        }

        public static string Parent(string path)
        {
            //TODO: refact
            path = path.TrimEnd('/');

            // cause we use >> as a sign of special command
            int cmdPos = path.IndexOf(">>", StringComparison.Ordinal);

            int pos = cmdPos > 0
                ? path.LastIndexOf("/", 0, cmdPos + 1, StringComparison.Ordinal)
                : path.LastIndexOf("/", StringComparison.Ordinal);

            return pos > 0
                ? path.Substring(0, pos)
                : "/";
        }

        public static string Name(string path)
        {
            //TODO: refact
            path = path.TrimEnd('/');

            // cause we use >> as a sign of special command
            int cmdPos = path.IndexOf(">>", StringComparison.Ordinal);

            int pos = cmdPos > 0
                    ? path.LastIndexOf("/", 0, cmdPos + 1, StringComparison.Ordinal)
                    : path.LastIndexOf("/", StringComparison.Ordinal);

            string res = path.Substring(pos+1);
            return res;
        }

        public static string Root => "/";
        public static string Separator => "/";

        public static WebDavPathParts Parts(string path)
        {
            //TODO: refact
            var res = new WebDavPathParts
            {
                Parent = Parent(path),
                Name = Name(path)
            };

            return res;
        }
    }

    public struct WebDavPathParts
    {
        public string Parent { get; set; }
        public string Name { get; set; }
    }
}
