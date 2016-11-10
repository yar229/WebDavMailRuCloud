using System;
using System.IO;
using System.Linq;
using System.Security;
using System.Threading.Tasks;
using MailRuCloudApi;
using NWebDav.Server.Http;
using NWebDav.Server.Locking;
using WebDavMailRuCloudStore;
using File = MailRuCloudApi.File;


namespace NWebDav.Server.Stores
{
    public sealed class MailruStore : IStore
    {
        public MailruStore(bool isWritable = true, ILockingManager lockingManager = null)
        {
            LockingManager = lockingManager ?? new InMemoryLockingManager();
            IsWritable = isWritable;
        }

        public bool IsWritable { get; private set; }
        public ILockingManager LockingManager { get; }

        public Task<IStoreItem> GetItemAsync(Uri uri, IHttpContext httpContext)
        {
            // Determine the path from the uri
            var path = GetPathFromUri(uri);

            try
            {
                var item = Cloud._cloud.GetItems(path).Result;
                if (item.FullPath == path)
                {
                    var dir = new Folder { FullPath = path };
                    return Task.FromResult<IStoreItem>(new MailruStoreCollection(LockingManager, dir, IsWritable));
                }
                else
                {
                    var f = item.Files.FirstOrDefault(k => k.FullPath == path);
                    return Task.FromResult<IStoreItem>(new MailruStoreItem(LockingManager, f, IsWritable));
                }
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
            var requestedPath = uri.LocalPath;
            requestedPath = requestedPath.TrimEnd('/');

            if (string.IsNullOrWhiteSpace(requestedPath)) requestedPath = "/";

            return requestedPath;
        }
    }
}
