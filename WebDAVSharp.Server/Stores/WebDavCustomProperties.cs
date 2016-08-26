using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebDAVSharp.Server.Stores
{
    /// <summary>
    /// Capture a series of WebDavCustomProperties grouped in the same namespace.
    /// </summary>
    public class WebDavCustomProperties
    {
        /// <summary>
        /// Namespace name, ex:http://ns.example.com/boxschema
        /// </summary>
        public String Namespace { get; set; }

        /// <summary>
        /// Namespace prefix, ex: N
        /// </summary>
        public String NamespacePrefix { get; set; }

        /// <summary>
        /// All properties grouped inside this namespace, in this first version we 
        /// care only about simple string properties.
        /// </summary>
        public List<WebDavProperty> Properties { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="namespece"></param>
        /// <param name="prefix"></param>
        public WebDavCustomProperties(String namespece, String prefix)
        {
            Namespace = namespece;
            NamespacePrefix = prefix;
            Properties = new List<WebDavProperty>();
        }
    }
}
