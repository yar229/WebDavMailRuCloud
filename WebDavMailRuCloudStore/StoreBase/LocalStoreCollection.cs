using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using NWebDav.Server;
using NWebDav.Server.Http;
using NWebDav.Server.Locking;
using NWebDav.Server.Logging;
using NWebDav.Server.Props;
using NWebDav.Server.Stores;
using YaR.Clouds.Base;
using File = YaR.Clouds.Base.File;

namespace YaR.Clouds.WebDavStore.StoreBase
{
    [DebuggerDisplay("{DirectoryInfo.FullPath}")]
    public class LocalStoreCollection : ILocalStoreCollection
    {
        private static readonly ILogger Logger = LoggerFactory.Factory.CreateLogger(typeof(LocalStoreCollection));
        private readonly IHttpContext _context;
        private readonly LocalStore _store;
        public Folder DirectoryInfo { get; }
        public IEntry EntryInfo => DirectoryInfo;
        public long Length => DirectoryInfo.Size;
        public bool IsReadable => false;

        public LocalStoreCollection(IHttpContext context, Folder directoryInfo, bool isWritable, 
            LocalStore store)
        {
            _context = context;
            DirectoryInfo = directoryInfo ?? throw new ArgumentNullException(nameof(directoryInfo));
            _store = store;

            IsWritable = isWritable;
        }

        public string CalculateEtag()
        {
            string h = DirectoryInfo.FullPath;
            var hash = SHA256.Create().ComputeHash(GetBytes(h));
            return BitConverter.ToString(hash).Replace("-", string.Empty);
        }

        private static byte[] GetBytes(string str)
        {
            byte[] bytes = new byte[str.Length * sizeof(char)];
            Buffer.BlockCopy(str.ToCharArray(), 0, bytes, 0, bytes.Length);
            return bytes;
        }

        //public PropertyManager<LocalStoreCollection> DefaultPropertyManager { get; }

        public bool IsWritable { get; }
        public string Name => DirectoryInfo.Name;
        public string UniqueKey => DirectoryInfo.FullPath;
        public string FullPath => DirectoryInfo.FullPath;

        public IPropertyManager PropertyManager => _store.CollectionPropertyManager;
        public ILockingManager LockingManager => _store.LockingManager;


        public IEnumerable<IStoreItem> Items
        {
            get
            {
                if (null != _items) 
                    return _items;

                lock (_itemsLocker)
                {
                    _items ??= GetItemsAsync(_context).Result;
                }
                return _items;
            }
        }

        private IEnumerable<IStoreItem> _items;
        private readonly object _itemsLocker = new();

        //public IEnumerable<LocalStoreCollection> Folders => Items.Where(it => it is LocalStoreCollection).Cast<LocalStoreCollection>();
        //public IEnumerable<LocalStoreItem> Files => Items.Where(it => it is LocalStoreItem).Cast<LocalStoreItem>();


        public Task<IStoreItem> GetItemAsync(string name, IHttpContext httpContext)
        {
            var res = name == string.Empty 
                ? this
                : Items.FirstOrDefault(i => i.Name == name);

            return Task.FromResult(res);
        }

        public Task<IEnumerable<IStoreItem>> GetItemsAsync(IHttpContext httpContext)
        {
            var list = DirectoryInfo.Entries
                .Select(entry => entry.IsFile
                    ? (IStoreItem)new LocalStoreItem((File)entry, IsWritable, _store)
                    : new LocalStoreCollection(httpContext, (Folder)entry, IsWritable, _store))
                .ToList();

            return Task.FromResult<IEnumerable<IStoreItem>>(list);
        }

        public Task<StoreItemResult> CreateItemAsync(string name, bool overwrite, IHttpContext httpContext)
        {
            if (!IsWritable)
                return Task.FromResult(new StoreItemResult(DavStatusCode.PreconditionFailed));

            var destinationPath = FullPath + "/" + name;

            var size = httpContext.Request.ContentLength();

            var f = new File(destinationPath, size);

            return Task.FromResult(new StoreItemResult(DavStatusCode.Created, new LocalStoreItem(f, IsWritable, _store)));
        }

        public Task<StoreCollectionResult> CreateCollectionAsync(string name, bool overwrite, IHttpContext httpContext)
        {
            if (!IsWritable)
                return Task.FromResult(new StoreCollectionResult(DavStatusCode.PreconditionFailed));

            var destinationPath = WebDavPath.Combine(FullPath, name);

            //var cmdFabric = new SpecialCommandFabric();
            //var cmd = cmdFabric.Build(CloudManager.Instance(httpContext.Session.Principal.Identity), destinationPath);
            //if (cmd != null)
            //{
            //    var res = cmd.Execute().Result;
            //    if (!res.IsSuccess)
            //        Logger.Log(LogLevel.Error, res.Message);

            //    return Task.FromResult(new StoreCollectionResult(res.IsSuccess ? DavStatusCode.Created : DavStatusCode.PreconditionFailed));
            //}

            DavStatusCode result;

            if (name != string.Empty && FindSubItem(name) != null)
            {
                if (!overwrite)
					// rclone tries to create folder on file copy and does not understand PreconditionFailed
					//return Task.FromResult(new StoreCollectionResult(DavStatusCode.PreconditionFailed));
					return Task.FromResult(new StoreCollectionResult(DavStatusCode.Created));

				result = DavStatusCode.NoContent;
            }
            else
                result = DavStatusCode.Created;

            try
            {
                CloudManager.Instance(httpContext.Session.Principal.Identity).CreateFolder(name, FullPath);
            }
            catch (Exception exc)
            {
                Logger.Log(LogLevel.Error, () => $"Unable to create '{destinationPath}' directory.", exc);
                return null;
            }

            return Task.FromResult(
                new StoreCollectionResult(result,
                new LocalStoreCollection(httpContext, new Folder(destinationPath), IsWritable, _store)));
        }

        public bool SupportsFastMove(IStoreCollection destination, string destinationName, bool overwrite, IHttpContext httpContext)
        {
            return false;
        }

        public Task<Stream> GetReadableStreamAsync(IHttpContext httpContext) => Task.FromResult((Stream)null);

        public Task<DavStatusCode> UploadFromStreamAsync(IHttpContext httpContext, Stream source)
        {
            throw new NotImplementedException("Cannot upload a collection by stream");
        }

        public async Task<StoreItemResult> CopyAsync(IStoreCollection destinationCollection, string name, bool overwrite, IHttpContext httpContext)
        {
            var instance = CloudManager.Instance(httpContext.Session.Principal.Identity);
            var res = await instance.Copy(DirectoryInfo, destinationCollection.GetFullPath());

            return new StoreItemResult( res ? DavStatusCode.Created : DavStatusCode.InternalServerError);
        }

        public async Task<StoreItemResult> MoveItemAsync(string sourceName, IStoreCollection destinationCollection, string destinationName, bool overwrite, IHttpContext httpContext)
        {
            // Return error
            if (!IsWritable)
                return new StoreItemResult(DavStatusCode.PreconditionFailed);

            // Determine the object that is being moved
            var item = await GetItemAsync(sourceName, httpContext);
            if (item == null)
                return new StoreItemResult(DavStatusCode.NotFound);

            var instance = CloudManager.Instance(httpContext.Session.Principal.Identity);

            if (destinationCollection is LocalStoreCollection destinationStoreCollection)
            {
                if (!destinationStoreCollection.IsWritable)
                    return new StoreItemResult(DavStatusCode.PreconditionFailed);

                var itemexist = destinationStoreCollection.FindSubItem(destinationName);

                DavStatusCode result;

                if (itemexist != null)
                {
                    if (!overwrite)
                        return new StoreItemResult(DavStatusCode.Forbidden);

                    await instance.Remove(itemexist);

                    result = DavStatusCode.NoContent;
                }
                else
                    result = DavStatusCode.Created;


                if (destinationStoreCollection.FullPath == FullPath)
                    await instance.Rename(item, destinationName);
                else
                {
                    if (sourceName == destinationName || string.IsNullOrEmpty(destinationName))
                    {
                        await instance.Move(item, destinationStoreCollection.FullPath);
                    }
                    else
                    {
                        var fi = ((LocalStoreItem)item).FileInfo;

                        string tmpName = Guid.NewGuid().ToString();
                        await instance.Rename(item, tmpName);
                        fi.SetName(tmpName);

                        await instance.MoveAsync(fi, destinationStoreCollection.FullPath);

                        fi.SetPath(destinationStoreCollection.FullPath);

                        await instance.Rename(fi, destinationName);
                    }
                }

                return new StoreItemResult(result, new LocalStoreItem(null, IsWritable, _store));
            }
            else
            {
                // Attempt to copy the item to the destination collection
                var result = await item.CopyAsync(destinationCollection, destinationName, overwrite, httpContext);
                if (result.Result is DavStatusCode.Created or DavStatusCode.NoContent)
                    await DeleteItemAsync(sourceName, httpContext);

                return result;
            }
        }

        private IStoreItem FindSubItem(string name)
        {
            return string.IsNullOrEmpty(name) ? this : Items.FirstOrDefault(it => it.Name == name);
        }

        public async Task<DavStatusCode> DeleteItemAsync(string name, IHttpContext httpContext)
        {
            if (!IsWritable)
                return DavStatusCode.PreconditionFailed;

            // Determine the full path
            var fullPath = WebDavPath.Combine(DirectoryInfo.FullPath, name);
            try
            {
                var item = FindSubItem(name);

                if (null == item) return DavStatusCode.NotFound;

                var cloud = CloudManager.Instance(httpContext.Session.Principal.Identity);
                bool res = await cloud.Remove(item);

                return res ? DavStatusCode.Ok : DavStatusCode.InternalServerError;
            }
            catch (Exception exc)
            {
                Logger.Log(LogLevel.Error, () => $"Unable to delete '{fullPath}' directory.", exc);
                return DavStatusCode.InternalServerError;
            }
        }

        public InfiniteDepthMode InfiniteDepthMode => InfiniteDepthMode.Allowed;
        public bool IsValid => !string.IsNullOrEmpty(DirectoryInfo?.FullPath);


        public override int GetHashCode()
        {
            return DirectoryInfo.FullPath.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            return obj is LocalStoreCollection storeCollection && 
                   storeCollection.DirectoryInfo.FullPath.Equals(DirectoryInfo.FullPath, StringComparison.CurrentCultureIgnoreCase);
        }
    }
}