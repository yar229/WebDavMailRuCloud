using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MailRuCloudApi;
using WebDAVSharp.Server.Stores;

namespace WebDavMailRuCloudStore
{
    /// <summary>
    /// This class implements a disk-based <see cref="IWebDavStore" />.
    /// </summary>
    [DebuggerDisplay("Disk Store ({RootPath})")]
    public sealed class WebDavMailRuCloudStore : IWebDavStore
    {
        //private readonly string _rootPath;
        private readonly string _login;
        private readonly string _password;
        private readonly MailRuCloud _cloud;


        //public MailRuCloud Cloud => _cloud;


        /// <summary>
        /// Initializes a new instance of the <see cref="WebDavMailRuCloudStore" /> class.
        /// </summary>
        /// <param name="login"></param>
        /// <param name="password"></param>
        /// <exception cref="System.ArgumentNullException"></exception>
        /// <exception cref="System.IO.DirectoryNotFoundException"></exception>
        public WebDavMailRuCloudStore(string login, string password)
        {
            if (login == null) throw new ArgumentNullException(nameof(login));
            if (password == null) throw new ArgumentNullException(nameof(password));

            _login = login;
            _password = password;

            _cloud = new MailRuCloud(login, password);
            
        }

        public string RootPath => string.Empty;

        #region IWebDAVStore Members

        /// <summary>
        /// Gets the root collection of this <see cref="IWebDavStore" />.
        /// </summary>
        public IWebDavStoreCollection Root => new WebDavMailRuCloudStoreCollection(_cloud, null, "/");

        #endregion
    }
}
