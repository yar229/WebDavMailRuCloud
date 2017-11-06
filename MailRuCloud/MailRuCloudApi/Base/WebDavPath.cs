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

        public static bool IsParentOrSame(string parent, string child)
        {
            parent = Clean(parent) + Separator;
            child = Clean(child) + Separator;
            return child.StartsWith(parent);

        }

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

        public static string ModifyParent(string path, string oldParent, string newParent)
        {
            if (!IsParentOrSame(oldParent, path))
                return path;

            path = Clean(path) + Separator;
            oldParent = Clean(oldParent) + Separator;

            path = path.Remove(0, oldParent.Length);

            return Combine(newParent, path);
        }

        public static bool PathEquals(string path1, string path2)
        {
            return String.Compare(Clean(path1), Clean(path2), StringComparison.InvariantCultureIgnoreCase) == 0;
        }
    }

    public struct WebDavPathParts
    {
        public string Parent { get; set; }
        public string Name { get; set; }
    }
}
