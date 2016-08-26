using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace WebDAVSharp.Server.Utilities
{
    /// <summary>
    /// Utilities for logging.
    /// </summary>
    public static class LoggingUtils
    {
        /// <summary>
        /// Pretty print xml
        /// </summary>
        /// <param name="doc"></param>
        /// <returns></returns>
        static public string Beautify(this XmlDocument doc)
        {
            if (doc == null) return "";
            StringBuilder sb = new StringBuilder();
            XmlWriterSettings settings = new XmlWriterSettings
            {
                Indent = true,
                IndentChars = "  ",
                NewLineChars = Environment.NewLine,
                NewLineHandling = NewLineHandling.Replace,
            };

            using (XmlWriter writer = XmlWriter.Create(sb, settings))
            {
                doc.Save(writer);
            }
            return sb.ToString();
        }
    }
}
