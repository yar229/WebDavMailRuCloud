using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using System.Xml.Linq;
using NWebDav.Server;
using NWebDav.Server.Http;
using NWebDav.Server.Locking;
using NWebDav.Server.Logging;
using NWebDav.Server.Props;
using NWebDav.Server.Stores;
using YaR.Clouds.Base;
using YaR.Clouds.WebDavStore.CustomProperties;
using File = YaR.Clouds.Base.File;

namespace YaR.Clouds.WebDavStore.StoreBase
{
    [DebuggerDisplay("{_directoryInfo.FullPath}")]
    public sealed class LocalStoreCollection : ILocalStoreCollection
    {
        private static readonly ILogger Logger = LoggerFactory.Factory.CreateLogger(typeof(LocalStoreCollection));
        private static readonly XElement SxDavCollection = new XElement(WebDavNamespaces.DavNs + "collection");
        private readonly IHttpContext _context;
        private readonly Folder _directoryInfo;
        public Folder DirectoryInfo => _directoryInfo;
        public IEntry EntryInfo => DirectoryInfo;
        public long Length => _directoryInfo.Size;
        public bool IsReadable => false;

        public LocalStoreCollection(IHttpContext context, ILockingManager lockingManager, Folder directoryInfo, bool isWritable)
        {
            LockingManager = lockingManager;
            _context = context;
            _directoryInfo = directoryInfo;
            IsWritable = isWritable;
        }

        private string CalculateEtag()
        {
            string h = _directoryInfo.FullPath;
            var hash = SHA256.Create().ComputeHash(GetBytes(h));
            return BitConverter.ToString(hash).Replace("-", string.Empty);
        }

        static byte[] GetBytes(string str)
        {
            byte[] bytes = new byte[str.Length * sizeof(char)];
            Buffer.BlockCopy(str.ToCharArray(), 0, bytes, 0, bytes.Length);
            return bytes;
        }

        public static PropertyManager<LocalStoreCollection> DefaultPropertyManager { get; } = new PropertyManager<LocalStoreCollection>(new DavProperty<LocalStoreCollection>[]
        {
            //// was added to to make WebDrive work, but no success
            //new DavHref<LocalStoreCollection>
            //{
            //    Getter = (context, collection) => collection._directoryInfo.Name
            //},

            //new DavLoctoken<LocalStoreCollection>
            //{
            //    Getter = (context, collection) => ""
            //},

            // collection property required for WebDrive
            new DavCollection<LocalStoreCollection>
            {
                Getter = (context, collection) => string.Empty
            },

            new DavGetEtag<LocalStoreCollection>
            {
                Getter = (context, item) => item.CalculateEtag()
            },

            //new DavBsiisreadonly<LocalStoreCollection>
            //{
            //    Getter = (context, item) => false
            //},

            //new DavSrtfileattributes<LocalStoreCollection>
            //{
            //    Getter = (context, collection) =>  collection.DirectoryInfo.Attributes,
            //    Setter = (context, collection, value) =>
            //    {
            //        collection.DirectoryInfo.Attributes = value;
            //        return DavStatusCode.Ok;
            //    }
            //},
            ////====================================================================================================
            

            new DavIsreadonly<LocalStoreCollection>
            {
                Getter = (context, item) => !item.IsWritable
            },

            new DavQuotaAvailableBytes<LocalStoreCollection>
            {
                Getter = (context, collection) => collection.FullPath == "/" ? CloudManager.Instance(context.Session.Principal.Identity).GetDiskUsage().Free.DefaultValue : long.MaxValue,
                IsExpensive = true  //folder listing performance
            },

            new DavQuotaUsedBytes<LocalStoreCollection>
            {
                Getter = (context, collection) => 
                    collection.DirectoryInfo.Size
                //IsExpensive = true  //folder listing performance
            },

            // RFC-2518 properties
            new DavCreationDate<LocalStoreCollection>
            {
                Getter = (context, collection) => collection._directoryInfo.CreationTimeUtc,
                Setter = (context, collection, value) =>
                {
                    collection._directoryInfo.CreationTimeUtc = value;
                    return DavStatusCode.Ok;
                }
            },
            new DavDisplayName<LocalStoreCollection>
            {
                Getter = (context, collection) => collection._directoryInfo.Name
            },
            new DavGetLastModified<LocalStoreCollection>
            {
                Getter = (context, collection) => collection._directoryInfo.LastWriteTimeUtc,
                Setter = (context, collection, value) =>
                {
                    collection._directoryInfo.LastWriteTimeUtc = value;
                    return DavStatusCode.Ok;
                }
            },

            new DavLastAccessed<LocalStoreCollection>
            {
                Getter = (context, collection) => collection._directoryInfo.LastWriteTimeUtc,
                Setter = (context, collection, value) =>
                {
                    collection._directoryInfo.LastWriteTimeUtc = value;
                    return DavStatusCode.Ok;
                }
            },


            //new DavGetResourceType<LocalStoreCollection>
            //{
            //    Getter = (context, collection) => new XElement(WebDavNamespaces.DavNs + "collection")
            //},
            new DavGetResourceType<LocalStoreCollection>
            {
                Getter = (context, collection) => new []{SxDavCollection}
            },


            // Default locking property handling via the LockingManager
            new DavLockDiscoveryDefault<LocalStoreCollection>(),
            new DavSupportedLockDefault<LocalStoreCollection>(),

            //Hopmann/Lippert collection properties
            new DavExtCollectionChildCount<LocalStoreCollection>
            {
                Getter = (context, collection) => collection.DirectoryInfo.NumberOfFolders + collection.DirectoryInfo.NumberOfFiles
            },
            new DavExtCollectionIsFolder<LocalStoreCollection>
            {
                Getter = (context, collection) => true
            },
            new DavExtCollectionIsHidden<LocalStoreCollection>
            {
                Getter = (context, collection) => false
            },
            new DavExtCollectionIsStructuredDocument<LocalStoreCollection>
            {
                Getter = (context, collection) => false
            },

            new DavExtCollectionHasSubs<LocalStoreCollection>
            {
                Getter = (context, collection) => collection.DirectoryInfo.NumberOfFolders > 0 
            },

            new DavExtCollectionNoSubs<LocalStoreCollection>
            {
                Getter = (context, collection) => false
            },

            new DavExtCollectionObjectCount<LocalStoreCollection>
            {
                Getter = (context, collection) => collection.DirectoryInfo.NumberOfFiles
            },

            new DavExtCollectionReserved<LocalStoreCollection>
            {
                Getter = (context, collection) => !collection.IsWritable
            },

            new DavExtCollectionVisibleCount<LocalStoreCollection>
            {
                Getter = (context, collection) => collection.DirectoryInfo.NumberOfFiles + collection.DirectoryInfo.NumberOfFolders
            },

            // Win32 extensions
            new Win32CreationTime<LocalStoreCollection>
            {
                Getter = (context, collection) => collection._directoryInfo.CreationTimeUtc,
                Setter = (context, collection, value) =>
                {
                    collection.DirectoryInfo.CreationTimeUtc = value;
                    return DavStatusCode.Ok;
                }
            },
            new Win32LastAccessTime<LocalStoreCollection>
            {
                Getter = (context, collection) => collection.DirectoryInfo.LastAccessTimeUtc,
                Setter = (context, collection, value) =>
                {
                    collection._directoryInfo.LastAccessTimeUtc = value;
                    return DavStatusCode.Ok;
                }
            },
            new Win32LastModifiedTime<LocalStoreCollection>
            {
                Getter = (context, collection) => collection.DirectoryInfo.LastWriteTimeUtc,
                Setter = (context, collection, value) =>
                {
                    collection.DirectoryInfo.LastWriteTimeUtc = value;
                    return DavStatusCode.Ok;
                }
            },
            new Win32FileAttributes<LocalStoreCollection>
            {
                Getter = (context, collection) =>  collection.DirectoryInfo.Attributes,
                Setter = (context, collection, value) =>
                {
                    collection.DirectoryInfo.Attributes = value;
                    return DavStatusCode.Ok;
                }
            },
            new DavGetContentLength<LocalStoreCollection>
            {
                Getter = (context, item) => item.DirectoryInfo.Size
            },
            new DavGetContentType<LocalStoreCollection>
            {
                Getter = (context, item) => "httpd/unix-directory" //"application/octet-stream"
            },
            new DavSharedLink<LocalStoreCollection>
            {
                Getter = (context, item) => !item.DirectoryInfo.PublicLinks.Any()
                    ? string.Empty
                    : item.DirectoryInfo.PublicLinks.First().Uri.OriginalString,
                Setter = (context, item, value) => DavStatusCode.Ok
            }
        });

        public bool IsWritable { get; }
        public string Name => DirectoryInfo.Name;
        public string UniqueKey => DirectoryInfo.FullPath;
        public string FullPath => DirectoryInfo.FullPath;

        public IPropertyManager PropertyManager => DefaultPropertyManager;
        public ILockingManager LockingManager { get; }


        public IList<IStoreItem> Items
        {
            get
            {
                if (null == _items)
                {
                    lock (_itemsLocker)
                    {
                        if (null == _items)
                        {
                            _items = GetItemsAsync(_context).Result;
                        }
                    }
                }
                return _items;
            }
        }

        private IList<IStoreItem> _items;
        private readonly object _itemsLocker = new object();

        public IEnumerable<LocalStoreCollection> Folders => Items.Where(it => it is LocalStoreCollection).Cast<LocalStoreCollection>();
        public IEnumerable<LocalStoreItem> Files => Items.Where(it => it is LocalStoreItem).Cast<LocalStoreItem>();


        public Task<IStoreItem> GetItemAsync(string name, IHttpContext httpContext)
        {
            var res = name == string.Empty 
                ? this
                : Items.FirstOrDefault(i => i.Name == name);

            return Task.FromResult(res);
        }

        public Task<IList<IStoreItem>> GetItemsAsync(IHttpContext httpContext)
        {
            var list = _directoryInfo.Entries
                .Select(entry => entry.IsFile
                    ? (IStoreItem) new LocalStoreItem(LockingManager, (File) entry, IsWritable)
                    : new LocalStoreCollection(httpContext, LockingManager, (Folder) entry, IsWritable))
                .ToList();

            return Task.FromResult<IList<IStoreItem>>(list);
        }

        public Task<StoreItemResult> CreateItemAsync(string name, bool overwrite, IHttpContext httpContext)
        {
            if (!IsWritable)
                return Task.FromResult(new StoreItemResult(DavStatusCode.PreconditionFailed));

            var destinationPath = FullPath + "/" + name;

            DavStatusCode result = DavStatusCode.Created;

            var size = httpContext.Request.ContentLength();

            var f = new File(destinationPath, size, null);

            return Task.FromResult(new StoreItemResult(result, new LocalStoreItem(LockingManager, f, IsWritable)));
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

            return Task.FromResult(new StoreCollectionResult(result, new LocalStoreCollection(httpContext, LockingManager, new Folder(destinationPath), IsWritable)));
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
            var res = await instance.Copy(_directoryInfo, destinationCollection.GetFullPath());

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

                return new StoreItemResult(result, new LocalStoreItem(LockingManager, null, IsWritable));
            }
            else
            {
                // Attempt to copy the item to the destination collection
                var result = await item.CopyAsync(destinationCollection, destinationName, overwrite, httpContext);
                if (result.Result == DavStatusCode.Created || result.Result == DavStatusCode.NoContent)
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
            var fullPath = WebDavPath.Combine(_directoryInfo.FullPath, name);
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

        public InfiniteDepthMode InfiniteDepthMode { get; } = InfiniteDepthMode.Allowed;
        public bool IsValid => !string.IsNullOrEmpty(DirectoryInfo?.FullPath);


        public override int GetHashCode()
        {
            return DirectoryInfo.FullPath.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (!(obj is LocalStoreCollection storeCollection))
                return false;
            return storeCollection._directoryInfo.FullPath.Equals(_directoryInfo.FullPath, StringComparison.CurrentCultureIgnoreCase);
        }
    }
}