using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using MailRuCloudApi;
using NWebDav.Server.Http;
using NWebDav.Server.Locking;
using NWebDav.Server.Stores;
using WebDavMailRuCloudStore;

namespace YaR.WebDavMailRu.CloudStore
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
            // Determine the path from the uri
            var path = GetPathFromUri(uri);

            try
            {
                var item = Cloud.Instance.GetItems(path).Result;
                if (item.FullPath == path)
                {
                    var dir = new Folder { FullPath = path };
                    return Task.FromResult<IStoreItem>(new MailruStoreCollection(LockingManager, dir, IsWritable));
                }

                var f = item.Files.FirstOrDefault(k => k.FullPath == path);
                return Task.FromResult<IStoreItem>(new MailruStoreItem(LockingManager, f, IsWritable));
            }
            catch (Exception)
            {
                return Task.FromResult<IStoreItem>(null);
            }
        }

        public Task<IStoreCollection> GetCollectionAsync(Uri uri, IHttpContext httpContext)
        {
            var path = GetPathFromUri(uri);
            return Task.FromResult<IStoreCollection>(new MailruStoreCollection(LockingManager, new Folder() {FullPath = path}, IsWritable));
        }

        private string GetPathFromUri(Uri uri)
        {
            ////can't use uri.LocalPath and so on cause of special signs

            var requestedPath = Regex.Replace(uri.AbsoluteUri, @"^http?://.*?/", string.Empty);
            requestedPath = "/" + requestedPath.TrimEnd('/');

            if (string.IsNullOrWhiteSpace(requestedPath)) requestedPath = "/";

            requestedPath = Uri.UnescapeDataString(requestedPath);

            return requestedPath;

            //var requestedPath = uri.LocalPath;
            //requestedPath = requestedPath.TrimEnd('/');
            //requestedPath = HttpUtility.UrlDecode(requestedPath);
            //if (string.IsNullOrWhiteSpace(requestedPath)) requestedPath = "/";
            //return requestedPath;
        }
    }
}
