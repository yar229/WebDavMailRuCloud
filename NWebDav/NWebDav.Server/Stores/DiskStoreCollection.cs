using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;

using NWebDav.Server.Http;
using NWebDav.Server.Locking;
using NWebDav.Server.Logging;
using NWebDav.Server.Props;

namespace NWebDav.Server.Stores
{
    [DebuggerDisplay("{_directoryInfo.FullPath}\\")]
    public sealed class DiskStoreCollection : IDiskStoreCollection
    {
        private static readonly ILogger s_log = LoggerFactory.CreateLogger(typeof(DiskStoreCollection));
        private static readonly XElement s_xDavCollection = new XElement(WebDavNamespaces.DavNs + "collection");
        private readonly DirectoryInfo _directoryInfo;

        public DiskStoreCollection(ILockingManager lockingManager, DirectoryInfo directoryInfo, bool isWritable)
        {
            LockingManager = lockingManager;
            _directoryInfo = directoryInfo;
            IsWritable = isWritable;
        }

        public static PropertyManager<DiskStoreCollection> DefaultPropertyManager { get; } = new PropertyManager<DiskStoreCollection>(new DavProperty<DiskStoreCollection>[]
        {
            // RFC-2518 properties
            new DavCreationDate<DiskStoreCollection>
            {
                Getter = (context, collection) => collection._directoryInfo.CreationTimeUtc,
                Setter = (context, collection, value) =>
                {
                    collection._directoryInfo.CreationTimeUtc = value;
                    return DavStatusCode.Ok;
                }
            },
            new DavDisplayName<DiskStoreCollection>
            {
                Getter = (context, collection) => collection._directoryInfo.Name
            },
            new DavGetLastModified<DiskStoreCollection>
            {
                Getter = (context, collection) => collection._directoryInfo.LastWriteTimeUtc,
                Setter = (context, collection, value) =>
                {
                    collection._directoryInfo.LastWriteTimeUtc = value;
                    return DavStatusCode.Ok;
                }
            },
            new DavGetResourceType<DiskStoreCollection>
            {
                Getter = (context, collection) => s_xDavCollection
            },

            // Default locking property handling via the LockingManager
            new DavLockDiscoveryDefault<DiskStoreCollection>(),
            new DavSupportedLockDefault<DiskStoreCollection>(),

            // Hopmann/Lippert collection properties
            new DavExtCollectionChildCount<DiskStoreCollection>
            {
                Getter = (context, collection) => collection._directoryInfo.EnumerateFiles().Count() + collection._directoryInfo.EnumerateDirectories().Count()
            },
            new DavExtCollectionIsFolder<DiskStoreCollection>
            {
                Getter = (context, collection) => true
            },
            new DavExtCollectionIsHidden<DiskStoreCollection>
            {
                Getter = (context, collection) => (collection._directoryInfo.Attributes & FileAttributes.Hidden) != 0
            },
            new DavExtCollectionIsStructuredDocument<DiskStoreCollection>
            {
                Getter = (context, collection) => false
            },
            new DavExtCollectionHasSubs<DiskStoreCollection>
            {
                Getter = (context, collection) => collection._directoryInfo.EnumerateDirectories().Any()
            },
            new DavExtCollectionNoSubs<DiskStoreCollection>
            {
                Getter = (context, collection) => false
            },
            new DavExtCollectionObjectCount<DiskStoreCollection>
            {
                Getter = (context, collection) => collection._directoryInfo.EnumerateFiles().Count()
            },
            new DavExtCollectionReserved<DiskStoreCollection>
            {
                Getter = (context, collection) => !collection.IsWritable
            },
            new DavExtCollectionVisibleCount<DiskStoreCollection>
            {
                Getter = (context, collection) =>
                    collection._directoryInfo.EnumerateDirectories().Count(di => (di.Attributes & FileAttributes.Hidden) == 0) +
                    collection._directoryInfo.EnumerateFiles().Count(fi => (fi.Attributes & FileAttributes.Hidden) == 0)
            },

            // Win32 extensions
            new Win32CreationTime<DiskStoreCollection>
            {
                Getter = (context, collection) => collection._directoryInfo.CreationTimeUtc,
                Setter = (context, collection, value) =>
                {
                    collection._directoryInfo.CreationTimeUtc = value;
                    return DavStatusCode.Ok;
                }
            },
            new Win32LastAccessTime<DiskStoreCollection>
            {
                Getter = (context, collection) => collection._directoryInfo.LastAccessTimeUtc,
                Setter = (context, collection, value) =>
                {
                    collection._directoryInfo.LastAccessTimeUtc = value;
                    return DavStatusCode.Ok;
                }
            },
            new Win32LastModifiedTime<DiskStoreCollection>
            {
                Getter = (context, collection) => collection._directoryInfo.LastWriteTimeUtc,
                Setter = (context, collection, value) =>
                {
                    collection._directoryInfo.LastWriteTimeUtc = value;
                    return DavStatusCode.Ok;
                }
            },
            new Win32FileAttributes<DiskStoreCollection>
            {
                Getter = (context, collection) => collection._directoryInfo.Attributes,
                Setter = (context, collection, value) =>
                {
                    collection._directoryInfo.Attributes = value;
                    return DavStatusCode.Ok;
                }
            }
        });

        public bool IsWritable { get; }
        public string Name => _directoryInfo.Name;
        public string UniqueKey => _directoryInfo.FullName;
        public string FullPath => _directoryInfo.FullName;

        // Disk collections (a.k.a. directories don't have their own data)
        public Task<Stream> GetReadableStreamAsync(IHttpContext httpContext) => Task.FromResult((Stream)null);
        public Task<DavStatusCode> UploadFromStreamAsync(IHttpContext httpContext, Stream inputStream) => Task.FromResult(DavStatusCode.Conflict);

        public IPropertyManager PropertyManager => DefaultPropertyManager;
        public ILockingManager LockingManager { get; }

        public Task<IStoreItem> GetItemAsync(string name, IHttpContext httpContext)
        {
            // Determine the full path
            var fullPath = Path.Combine(_directoryInfo.FullName, name);

            // Check if the item is a file
            if (File.Exists(fullPath))
                return Task.FromResult<IStoreItem>(new DiskStoreItem(LockingManager, new FileInfo(fullPath), IsWritable));

            // Check if the item is a directory
            if (Directory.Exists(fullPath))
                return Task.FromResult<IStoreItem>(new DiskStoreCollection(LockingManager, new DirectoryInfo(fullPath), IsWritable));

            // Item not found
            return Task.FromResult<IStoreItem>(null);
        }

        public Task<IList<IStoreItem>> GetItemsAsync(IHttpContext httpContext)
        {
            var items = new List<IStoreItem>();

            // Add all directories
            foreach (var subDirectory in _directoryInfo.GetDirectories())
                items.Add(new DiskStoreCollection(LockingManager, subDirectory, IsWritable));

            // Add all files
            foreach (var file in _directoryInfo.GetFiles())
                items.Add(new DiskStoreItem(LockingManager, file, IsWritable));

            // Return the items
            return Task.FromResult<IList<IStoreItem>>(items);
        }

        public Task<StoreItemResult> CreateItemAsync(string name, bool overwrite, IHttpContext httpContext)
        {
            // Return error
            if (!IsWritable)
                return Task.FromResult(new StoreItemResult(DavStatusCode.PreconditionFailed));

            // Determine the destination path
            var destinationPath = Path.Combine(FullPath, name);

            // Determine result
            DavStatusCode result;

            // Check if the file can be overwritten
            if (File.Exists(name))
            {
                if (!overwrite)
                    return Task.FromResult(new StoreItemResult(DavStatusCode.PreconditionFailed));

                result = DavStatusCode.NoContent;
            }
            else
            {
                result = DavStatusCode.Created;
            }

            try
            {
                // Create a new file
                File.Create(destinationPath).Dispose();
            }
            catch (Exception exc)
            {
                // Log exception
                s_log.Log(LogLevel.Error, () => $"Unable to create '{destinationPath}' file.", exc);
                return Task.FromResult(new StoreItemResult(DavStatusCode.InternalServerError));
            }

            // Return result
            return Task.FromResult(new StoreItemResult(result, new DiskStoreItem(LockingManager, new FileInfo(destinationPath), IsWritable)));
        }

        public Task<StoreCollectionResult> CreateCollectionAsync(string name, bool overwrite, IHttpContext httpContext)
        {
            // Return error
            if (!IsWritable)
                return Task.FromResult(new StoreCollectionResult(DavStatusCode.PreconditionFailed));

            // Determine the destination path
            var destinationPath = Path.Combine(FullPath, name);

            // Check if the directory can be overwritten
            DavStatusCode result;
            if (Directory.Exists(destinationPath))
            {
                // Check if overwrite is allowed
                if (!overwrite)
                    return Task.FromResult(new StoreCollectionResult(DavStatusCode.PreconditionFailed));

                // Overwrite existing
                result = DavStatusCode.NoContent;
            }
            else
            {
                // Created new directory
                result = DavStatusCode.Created;
            }

            try
            {
                // Attempt to create the directory
                Directory.CreateDirectory(destinationPath);
            }
            catch (Exception exc)
            {
                // Log exception
                s_log.Log(LogLevel.Error, () => $"Unable to create '{destinationPath}' directory.", exc);
                return null;
            }

            // Return the collection
            return Task.FromResult(new StoreCollectionResult(result, new DiskStoreCollection(LockingManager, new DirectoryInfo(destinationPath), IsWritable)));
        }

        public async Task<StoreItemResult> CopyAsync(IStoreCollection destinationCollection, string name, bool overwrite, IHttpContext httpContext)
        {
            // Just create the folder itself
            var result = await destinationCollection.CreateCollectionAsync(name, overwrite, httpContext);
            return new StoreItemResult(result.Result, result.Collection);
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

            // Check if the item is actually a file
            var diskStoreItem = item as DiskStoreItem;
            if (diskStoreItem != null)
            {
                // If the destination collection is a directory too, then we can simply move the file
                var destinationDiskStoreCollection = destinationCollection as DiskStoreCollection;
                if (destinationDiskStoreCollection != null)
                {
                    // Return error
                    if (!destinationDiskStoreCollection.IsWritable)
                        return new StoreItemResult(DavStatusCode.PreconditionFailed);

                    // Determine source and destination paths
                    var sourcePath = Path.Combine(_directoryInfo.FullName, sourceName);
                    var destinationPath = Path.Combine(destinationDiskStoreCollection._directoryInfo.FullName, destinationName);

                    // Check if the file already exists
                    DavStatusCode result;
                    if (File.Exists(destinationPath))
                    {
                        // Remove the file if it already exists (if allowed)
                        if (!overwrite)
                            return new StoreItemResult(DavStatusCode.Forbidden);

                        // The file will be overwritten
                        File.Delete(destinationPath);
                        result = DavStatusCode.NoContent;
                    }
                    else
                    {
                        // The file will be "created"
                        result = DavStatusCode.Created;
                    }

                    // Move the file
                    File.Move(sourcePath, destinationPath);
                    return new StoreItemResult(result, new DiskStoreItem(LockingManager, new FileInfo(destinationPath), IsWritable));
                }
                else
                {
                    // Attempt to copy the item to the destination collection
                    var result = await item.CopyAsync(destinationCollection, destinationName, overwrite, httpContext);
                    if (result.Result == DavStatusCode.Created || result.Result == DavStatusCode.NoContent)
                        await DeleteItemAsync(sourceName, httpContext);

                    // Return the result
                    return result;
                }
            }

            // If it's not a plain item, then it's a collection
            Debug.Assert(item is DiskStoreCollection);

            // Collections will never be moved, but always be created
            // (we always move the individual items to make sure locking is checked properly)
            throw new InvalidOperationException("Collections should never be moved directly.");
        }

        public Task<DavStatusCode> DeleteItemAsync(string name, IHttpContext httpContext)
        {
            // Return error
            if (!IsWritable)
                return Task.FromResult(DavStatusCode.PreconditionFailed);

            // Determine the full path
            var fullPath = Path.Combine(_directoryInfo.FullName, name);
            try
            {
                // Check if the file exists
                if (File.Exists(fullPath))
                {
                    // Delete the file
                    File.Delete(fullPath);
                    return Task.FromResult(DavStatusCode.Ok);
                }

                // Check if the directory exists
                if (Directory.Exists(fullPath))
                {
                    // Delete the directory
                    Directory.Delete(fullPath);
                    return Task.FromResult(DavStatusCode.Ok);
                }

                // Item not found
                return Task.FromResult(DavStatusCode.NotFound);
            }
            catch (Exception exc)
            {
                // Log exception
                s_log.Log(LogLevel.Error, () => $"Unable to delete '{fullPath}' directory.", exc);
                return Task.FromResult(DavStatusCode.InternalServerError);
            }
        }

        public InfiniteDepthMode InfiniteDepthMode => InfiniteDepthMode.Rejected;

        public override int GetHashCode()
        {
            return _directoryInfo.FullName.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            var storeCollection = obj as DiskStoreCollection;
            if (storeCollection == null)
                return false;
            return storeCollection._directoryInfo.FullName.Equals(_directoryInfo.FullName, StringComparison.CurrentCultureIgnoreCase);
        }
    }
}