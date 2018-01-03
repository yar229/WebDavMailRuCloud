using System;

namespace YaR.MailRuCloud.Fs
{
    static class Path
    {
        public static String GetDirectoryName(String path)
        {
            int index = path.LastIndexOf('\\');
            if (0 > index)
                return path;
            else if (0 == index)
                return "\\";
            else
                return path.Substring(0, index);
        }

        public static String GetFileName(String path)
        {
            int index = path.LastIndexOf('\\');
            if (0 > index)
                return path;
            else
                return path.Substring(index + 1);
        }
    }
}