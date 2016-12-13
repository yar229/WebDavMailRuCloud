namespace YaR.WebDavMailRu.CloudStore
{
    public static class WebDavPath
    {
        public static string Combine(string a, string b)
        {
            a = Clean(a);
            b = Clean(b);
            a = a.Trim('/');
            b = b.TrimStart('/');
            string res = "/" + a + (string.IsNullOrEmpty(b) ? "" : "/" + b);
            return res;

        }

        public static string Clean(string path)
        {
            return path.Replace("\\", "/");
        }

    }
}
