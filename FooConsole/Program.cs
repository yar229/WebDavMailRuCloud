using System;
using WebDAVSharp.Server;

namespace FooConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            int port;
            int.TryParse(args[0], out port);

            string login = args[1].Trim('\"');
            string password = args[2].Trim('\"');

            //IWebDavStoreItemLock lockSystem = new WebDavStoreItemLock();
            var store = new WebDavMailRuCloudStore.WebDavMailRuCloudStore(login, password);
            //var store = new WebDavDiskStore("d:\\5");

            
            WebDavServer wds = new WebDavServer(store, AuthType.Anonymous);
            
            wds.Start("http://localhost:3333/");

        }
    }
}
