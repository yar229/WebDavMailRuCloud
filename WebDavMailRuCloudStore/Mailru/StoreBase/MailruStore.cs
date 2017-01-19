using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using MailRuCloudApi;
using NWebDav.Server.Http;
using NWebDav.Server.Locking;
using NWebDav.Server.Stores;

namespace YaR.WebDavMailRu.CloudStore.Mailru.StoreBase
{
    public sealed class MailruStore : IStore
    {
        public MailruStore(bool isWritable = true, ILockingManager lockingManager = null)
        {
            LockingManager = lockingManager ?? new InMemoryLockingManager();
            IsWritable = isWritable;
        }

        private bool IsWritable { get; }
        private ILockingManager LockingManager { get; }

        public Task<IStoreItem> GetItemAsync(Uri uri, IHttpContext httpContext)
        {

            //TODO: Refact
            // if GET - suggest file, PROPFIND - suggest folder

            var path = GetPathFromUri(uri);
            //var dirpath = httpContext.Request.HttpMethod == "GET"
            //    ? WebDavPath.Parent(path) 
            //    : path;


            //if (httpContext.Request.HttpMethod == "GET")
            //{
            //    var dir = Cloud.Instance.GetItems(dirpath).Result;
            //    var f = dir.Files.FirstOrDefault(k => k.FullPath == path);
            //    return Task.FromResult<IStoreItem>(new MailruStoreItem(LockingManager, f, IsWritable));
            //}

            //var dire = new Folder(dirpath);
            //return Task.FromResult<IStoreItem>(new MailruStoreCollection(LockingManager, dire, IsWritable));

            try
            {
                var item = Cloud.Instance.GetItems(path).Result;
                if (item.FullPath == path)
                {
                    var dir = new Folder(path);
                    return Task.FromResult<IStoreItem>(new MailruStoreCollection(LockingManager, dir, IsWritable));
                }

                var f = item.Files.FirstOrDefault(k => k.FullPath == path);
                return Task.FromResult<IStoreItem>(new MailruStoreItem(LockingManager, f, IsWritable));
            }
            catch (Exception ex)
            {
                return Task.FromResult<IStoreItem>(null);
            }
        }

        public Task<IStoreCollection> GetCollectionAsync(Uri uri, IHttpContext httpContext)
        {
            var path = GetPathFromUri(uri);
            return Task.FromResult<IStoreCollection>(new MailruStoreCollection(LockingManager, new Folder(path), IsWritable));
        }

        private string GetPathFromUri(Uri uri)
        {
            ////can't use uri.LocalPath and so on cause of special signs

            //var requestedPath = Regex.Replace(uri.AbsoluteUri, @"^http?://.*?/", string.Empty);
            var requestedPath = Regex.Replace(uri.OriginalString, @"^http?://.*?(/|\Z)", string.Empty);
            requestedPath = "/" + requestedPath.TrimEnd('/');

            if (string.IsNullOrWhiteSpace(requestedPath)) requestedPath = "/";

            requestedPath = Uri.UnescapeDataString(requestedPath);

            return requestedPath;
        }
    }
}
