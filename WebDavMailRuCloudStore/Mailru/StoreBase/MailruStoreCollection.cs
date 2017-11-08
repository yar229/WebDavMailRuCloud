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
using YaR.MailRuCloud.Api.Base;
using YaR.MailRuCloud.Api.SpecialCommands;
using YaR.WebDavMailRu.CloudStore.DavCustomProperty;
using File = YaR.MailRuCloud.Api.Base.File;

namespace YaR.WebDavMailRu.CloudStore.Mailru.StoreBase
{
    [DebuggerDisplay("{_directoryInfo.FullPath}")]
    public sealed class MailruStoreCollection : IMailruStoreCollection
    {
        private static readonly ILogger Logger = LoggerFactory.Factory.CreateLogger(typeof(MailruStoreCollection));
        private readonly IHttpContext _context;
        private readonly Folder _directoryInfo;
        public Folder DirectoryInfo => _directoryInfo;

        public MailruStoreCollection(IHttpContext context, ILockingManager lockingManager, Folder directoryInfo, bool isWritable)
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

        public static PropertyManager<MailruStoreCollection> DefaultPropertyManager { get; } = new PropertyManager<MailruStoreCollection>(new DavProperty<MailruStoreCollection>[]
        {
            //// was added to to make WebDrive work, but no success
            //new DavHref<MailruStoreCollection>
            //{
            //    Getter = (context, collection) => collection._directoryInfo.Name
            //},

            //new DavLoctoken<MailruStoreCollection>
            //{
            //    Getter = (context, collection) => ""
            //},

            // collection property required for WebDrive
            new DavCollection<MailruStoreCollection>
            {
                Getter = (context, collection) => string.Empty
            },

            new DavGetEtag<MailruStoreCollection>
            {
                Getter = (context, item) => item.CalculateEtag()
            },

            //new DavBsiisreadonly<MailruStoreCollection>
            //{
            //    Getter = (context, item) => false
            //},

            //new DavSrtfileattributes<MailruStoreCollection>
            //{
            //    Getter = (context, collection) =>  collection.DirectoryInfo.Attributes,
            //    Setter = (context, collection, value) =>
            //    {
            //        collection.DirectoryInfo.Attributes = value;
            //        return DavStatusCode.Ok;
            //    }
            //},
            ////====================================================================================================
            

            new DavIsreadonly<MailruStoreCollection>
            {
                Getter = (context, item) => !item.IsWritable
            },

            new DavQuotaAvailableBytes<MailruStoreCollection>
            {
                Getter = (context, collection) => collection.FullPath == "/" ? CloudManager.Instance(context.Session.Principal.Identity).GetDiskUsage().Result.Free.DefaultValue : long.MaxValue,
                IsExpensive = true  //folder listing performance
            },

            new DavQuotaUsedBytes<MailruStoreCollection>
            {
                Getter = (context, collection) => 
                    collection.DirectoryInfo.Size
                //IsExpensive = true  //folder listing performance
            },

            // RFC-2518 properties
            new DavCreationDate<MailruStoreCollection>
            {
                Getter = (context, collection) => collection._directoryInfo.CreationTimeUtc,
                Setter = (context, collection, value) =>
                {
                    collection._directoryInfo.CreationTimeUtc = value;
                    return DavStatusCode.Ok;
                }
            },
            new DavDisplayName<MailruStoreCollection>
            {
                Getter = (context, collection) => collection._directoryInfo.Name
            },
            new DavGetLastModified<MailruStoreCollection>
            {
                Getter = (context, collection) => collection._directoryInfo.LastWriteTimeUtc,
                Setter = (context, collection, value) =>
                {
                    collection._directoryInfo.LastWriteTimeUtc = value;
                    return DavStatusCode.Ok;
                }
            },

            new DavLastAccessed<MailruStoreCollection>
            {
                Getter = (context, collection) => collection._directoryInfo.LastWriteTimeUtc,
                Setter = (context, collection, value) =>
                {
                    collection._directoryInfo.LastWriteTimeUtc = value;
                    return DavStatusCode.Ok;
                }
            },


            new DavGetResourceType<MailruStoreCollection>
            {
                Getter = (context, collection) => new XElement(WebDavNamespaces.DavNs + "collection")
            },

            // Default locking property handling via the LockingManager
            new DavLockDiscoveryDefault<MailruStoreCollection>(),
            new DavSupportedLockDefault<MailruStoreCollection>(),

            //Hopmann/Lippert collection properties
            new DavExtCollectionChildCount<MailruStoreCollection>
            {
                Getter = (context, collection) => collection.DirectoryInfo.NumberOfFolders + collection.DirectoryInfo.NumberOfFiles
            },
            new DavExtCollectionIsFolder<MailruStoreCollection>
            {
                Getter = (context, collection) => true
            },
            new DavExtCollectionIsHidden<MailruStoreCollection>
            {
                Getter = (context, collection) => false
            },
            new DavExtCollectionIsStructuredDocument<MailruStoreCollection>
            {
                Getter = (context, collection) => false
            },

            new DavExtCollectionHasSubs<MailruStoreCollection>
            {
                Getter = (context, collection) => collection.DirectoryInfo.NumberOfFolders > 0 
            },

            new DavExtCollectionNoSubs<MailruStoreCollection>
            {
                Getter = (context, collection) => false
            },

            new DavExtCollectionObjectCount<MailruStoreCollection>
            {
                Getter = (context, collection) => collection.DirectoryInfo.NumberOfFiles
            },

            new DavExtCollectionReserved<MailruStoreCollection>
            {
                Getter = (context, collection) => !collection.IsWritable
            },

            new DavExtCollectionVisibleCount<MailruStoreCollection>
            {
                Getter = (context, collection) => collection.DirectoryInfo.NumberOfFiles + collection.DirectoryInfo.NumberOfFolders
            },

            // Win32 extensions
            new Win32CreationTime<MailruStoreCollection>
            {
                Getter = (context, collection) => collection._directoryInfo.CreationTimeUtc,
                Setter = (context, collection, value) =>
                {
                    collection.DirectoryInfo.CreationTimeUtc = value;
                    return DavStatusCode.Ok;
                }
            },
            new Win32LastAccessTime<MailruStoreCollection>
            {
                Getter = (context, collection) => collection.DirectoryInfo.LastAccessTimeUtc,
                Setter = (context, collection, value) =>
                {
                    collection._directoryInfo.LastAccessTimeUtc = value;
                    return DavStatusCode.Ok;
                }
            },
            new Win32LastModifiedTime<MailruStoreCollection>
            {
                Getter = (context, collection) => collection.DirectoryInfo.LastWriteTimeUtc,
                Setter = (context, collection, value) =>
                {
                    collection.DirectoryInfo.LastWriteTimeUtc = value;
                    return DavStatusCode.Ok;
                }
            },
            new Win32FileAttributes<MailruStoreCollection>
            {
                Getter = (context, collection) =>  collection.DirectoryInfo.Attributes,
                Setter = (context, collection, value) =>
                {
                    collection.DirectoryInfo.Attributes = value;
                    return DavStatusCode.Ok;
                }
            },
            new DavSharedLink<MailruStoreCollection>
            {
                Getter = (context, item) => item.DirectoryInfo.PublicLink,
                Setter = (context, item, value) => DavStatusCode.Ok
            },
            new DavGetContentLength<MailruStoreCollection>
            {
                Getter = (context, item) => item.DirectoryInfo.Size
            },
            new DavGetContentType<MailruStoreCollection>
            {
                Getter = (context, item) => "httpd/unix-directory" //"application/octet-stream"
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

        public IEnumerable<MailruStoreCollection> Folders => Items.Where(it => it is MailruStoreCollection).Cast<MailruStoreCollection>();
        public IEnumerable<MailruStoreItem> Files => Items.Where(it => it is MailruStoreItem).Cast<MailruStoreItem>();


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
                    ? (IStoreItem) new MailruStoreItem(LockingManager, (File) entry, IsWritable)
                    : new MailruStoreCollection(httpContext, LockingManager, (Folder) entry, IsWritable))
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

            return Task.FromResult(new StoreItemResult(result, new MailruStoreItem(LockingManager, f, IsWritable)));
        }

        public Task<StoreCollectionResult> CreateCollectionAsync(string name, bool overwrite, IHttpContext httpContext)
        {
            if (!IsWritable)
                return Task.FromResult(new StoreCollectionResult(DavStatusCode.PreconditionFailed));

            var destinationPath = WebDavPath.Combine(FullPath, name);

            var cmdFabric = new SpecialCommandFabric();
            var cmd = cmdFabric.Build(CloudManager.Instance(httpContext.Session.Principal.Identity), destinationPath);
            if (cmd != null)
            {
                var res = cmd.Execute().Result;
                return Task.FromResult(new StoreCollectionResult(res.IsSuccess ? DavStatusCode.Created : DavStatusCode.PreconditionFailed));
            }

            DavStatusCode result;

            if (name != string.Empty && FindSubItem(name) != null)
            {
                if (!overwrite)
                    return Task.FromResult(new StoreCollectionResult(DavStatusCode.PreconditionFailed));

                result = DavStatusCode.NoContent;
            }
            else
                result = DavStatusCode.Created;

            try
            {
                CloudManager.Instance(httpContext.Session.Principal.Identity).CreateFolder(name, FullPath).Wait();
            }
            catch (Exception exc)
            {
                Logger.Log(LogLevel.Error, () => $"Unable to create '{destinationPath}' directory.", exc);
                return null;
            }

            return Task.FromResult(new StoreCollectionResult(result, new MailruStoreCollection(httpContext, LockingManager, new Folder(destinationPath), IsWritable)));
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

            return new StoreItemResult( res ? DavStatusCode.Created : DavStatusCode.InternalServerError, null);
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

            if (destinationCollection is MailruStoreCollection destinationStoreCollection)
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
                        var fi = ((MailruStoreItem)item).FileInfo;

                        string tmpName = Guid.NewGuid().ToString();
                        await instance.Rename(item, tmpName);
                        fi.SetName(tmpName);

                        await instance.Move(fi, destinationStoreCollection.FullPath);

                        fi.SetPath(destinationStoreCollection.FullPath);

                        await instance.Rename(fi, destinationName);
                    }
                }

                return new StoreItemResult(result, new MailruStoreItem(LockingManager, null, IsWritable));
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

        public override int GetHashCode()
        {
            return DirectoryInfo.FullPath.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (!(obj is MailruStoreCollection storeCollection))
                return false;
            return storeCollection._directoryInfo.FullPath.Equals(_directoryInfo.FullPath, StringComparison.CurrentCultureIgnoreCase);
        }
    }
}