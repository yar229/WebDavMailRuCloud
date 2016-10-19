using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;
using MailRuCloudApi;
using NWebDav.Server.Http;
using NWebDav.Server.Locking;
using NWebDav.Server.Logging;
using NWebDav.Server.Props;
using WebDavMailRuCloudStore;
using File = System.IO.File;

namespace NWebDav.Server.Stores
{
    [DebuggerDisplay("{_directoryInfo.FullPath}\\")]
    public sealed class MailruStoreCollection : IMailruStoreCollection
    {
        private static readonly ILogger s_log = LoggerFactory.CreateLogger(typeof(MailruStoreCollection));
        private readonly Folder _directoryInfo;

        public MailruStoreCollection(ILockingManager lockingManager, Folder directoryInfo, bool isWritable)
        {
            LockingManager = lockingManager;
            _directoryInfo = directoryInfo;
            IsWritable = isWritable;
        }

        public static PropertyManager<MailruStoreCollection> DefaultPropertyManager { get; } = new PropertyManager<MailruStoreCollection>(new DavProperty<MailruStoreCollection>[]
        {
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
                Getter = (context, collection) => 
                collection._directoryInfo.Name
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
            new DavGetResourceType<MailruStoreCollection>
            {
                Getter = (context, collection) => new XElement(WebDavNamespaces.DavNs + "collection")
            },

            // Default locking property handling via the LockingManager
            new DavLockDiscoveryDefault<MailruStoreCollection>(),
            new DavSupportedLockDefault<MailruStoreCollection>(),

            // Hopmann/Lippert collection properties
            new DavExtCollectionChildCount<MailruStoreCollection>
            {
                Getter = (context, collection) =>
                {
                    //collection._directoryInfo.EnumerateFiles().Count() + collection._directoryInfo.EnumerateDirectories().Count()
                    var data = Cloud._cloud.GetItems(collection._directoryInfo).Result;
                    int cnt = data.NumberOfItems;
                    return cnt;
                }
                
                
            },
            new DavExtCollectionIsFolder<MailruStoreCollection>
            {
                Getter = (context, collection) => true
            },
            new DavExtCollectionIsHidden<MailruStoreCollection>
            {
                Getter = (context, collection) => false //(collection._directoryInfo.Attributes & FileAttributes.Hidden) != 0
            },
            new DavExtCollectionIsStructuredDocument<MailruStoreCollection>
            {
                Getter = (context, collection) => false
            },
            new DavExtCollectionHasSubs<MailruStoreCollection>
            {
                Getter = (context, collection) =>
                {
                    //collection._directoryInfo.EnumerateDirectories().Any()

                    var data = Cloud._cloud.GetItems(collection._directoryInfo).Result;
                    return data.NumberOfFolders > 0;
                }
                
            },
            new DavExtCollectionNoSubs<MailruStoreCollection>
            {
                Getter = (context, collection) => false
            },
            new DavExtCollectionObjectCount<MailruStoreCollection>
            {
                Getter = (context, collection) =>
                {
                    //collection._directoryInfo.EnumerateFiles().Count()
                    var data = Cloud._cloud.GetItems(collection._directoryInfo).Result;
                    int cnt = data.NumberOfFiles;
                    return cnt;
                }
            },
            new DavExtCollectionReserved<MailruStoreCollection>
            {
                Getter = (context, collection) => !collection.IsWritable
            },
            new DavExtCollectionVisibleCount<MailruStoreCollection>
            {
                Getter = (context, collection) =>
                {
                    //collection._directoryInfo.EnumerateDirectories().Count(di => (di.Attributes & FileAttributes.Hidden) == 0) +
                    //collection._directoryInfo.EnumerateFiles().Count(fi => (fi.Attributes & FileAttributes.Hidden) == 0)
                    var data = Cloud._cloud.GetItems(collection._directoryInfo).Result;
                    return data.NumberOfItems;
                }
            },

            // Win32 extensions
            new Win32CreationTime<MailruStoreCollection>
            {
                Getter = (context, collection) => collection._directoryInfo.CreationTimeUtc,
                Setter = (context, collection, value) =>
                {
                    collection._directoryInfo.CreationTimeUtc = value;
                    return DavStatusCode.Ok;
                }
            },
            new Win32LastAccessTime<MailruStoreCollection>
            {
                Getter = (context, collection) => collection._directoryInfo.LastAccessTimeUtc,
                Setter = (context, collection, value) =>
                {
                    collection._directoryInfo.LastAccessTimeUtc = value;
                    return DavStatusCode.Ok;
                }
            },
            new Win32LastModifiedTime<MailruStoreCollection>
            {
                Getter = (context, collection) => collection._directoryInfo.LastWriteTimeUtc,
                Setter = (context, collection, value) =>
                {
                    collection._directoryInfo.LastWriteTimeUtc = value;
                    return DavStatusCode.Ok;
                }
            },
            new Win32FileAttributes<MailruStoreCollection>
            {
                Getter = (context, collection) => 
                    collection._directoryInfo.Attributes,
                Setter = (context, collection, value) =>
                {
                    collection._directoryInfo.Attributes = value;
                    return DavStatusCode.Ok;
                }
            }
        });

        public bool IsWritable { get; }
        public string Name => _directoryInfo.Name;
        public string UniqueKey => _directoryInfo.FullPath;
        public string FullPath => _directoryInfo.FullPath;

        // Disk collections (a.k.a. directories don't have their own data)
        public Stream GetReadableStream(IHttpContext httpContext) => null;
        public Stream GetWritableStream(IHttpContext httpContext) => null;

        public IPropertyManager PropertyManager => DefaultPropertyManager;
        public ILockingManager LockingManager { get; }

        public Task<IStoreItem> GetItemAsync(string name, IHttpContext httpContext)
        {
            // Determine the full path
            var fullPath = Path.Combine(_directoryInfo.FullPath, name);
            // Check if the item is a file
            //if (File.Exists(fullPath))
            //    return Task.FromResult<IStoreItem>(new MailruStoreItem(LockingManager, new FileInfo(fullPath), IsWritable));

            // Check if the item is a directory
            //if (Directory.Exists(fullPath))
            return Task.FromResult<IStoreItem>(new MailruStoreCollection(LockingManager, new Folder {FullPath = fullPath}, IsWritable));

            // Item not found
            //return Task.FromResult<IStoreItem>(null);
        }

        public Task<IList<IStoreItem>> GetItemsAsync(IHttpContext httpContext)
        {
            var item = Cloud._cloud.GetItems(_directoryInfo).Result;

            // Add all directories
            var items = item.Folders.Select(subDirectory => new MailruStoreCollection(LockingManager, subDirectory, IsWritable)).Cast<IStoreItem>().ToList();

            // Add all files
            items.AddRange(item.Files.Select(file => new MailruStoreItem(LockingManager, file, IsWritable)));


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
                s_log.Log(LogLevel.Error, $"Unable to create '{destinationPath}' file.", exc);
                return Task.FromResult(new StoreItemResult(DavStatusCode.InternalServerError));
            }

            // Return result
            //return Task.FromResult(new StoreItemResult(result, new MailruStoreItem(LockingManager, new File(destinationPath), IsWritable)));
            return Task.FromResult(new StoreItemResult(result, new MailruStoreItem(LockingManager, new MailRuCloudApi.File(), IsWritable)));
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
                s_log.Log(LogLevel.Error, $"Unable to create '{destinationPath}' directory.", exc);
                return null;
            }

            // Return the collection
            //return Task.FromResult(new StoreCollectionResult(result, new MailruStoreCollection(LockingManager, new DirectoryInfo(destinationPath), IsWritable)));
            return Task.FromResult(new StoreCollectionResult(result, new MailruStoreCollection(LockingManager, new Folder(), IsWritable)));
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
            var diskStoreItem = item as MailruStoreItem;
            if (diskStoreItem != null)
            {
                // If the destination collection is a directory too, then we can simply move the file
                var destinationDiskStoreCollection = destinationCollection as MailruStoreCollection;
                if (destinationDiskStoreCollection != null)
                {
                    // Return error
                    if (!destinationDiskStoreCollection.IsWritable)
                        return new StoreItemResult(DavStatusCode.PreconditionFailed);

                    // Determine source and destination paths
                    var sourcePath = Path.Combine(_directoryInfo.FullPath, sourceName);
                    var destinationPath = Path.Combine(destinationDiskStoreCollection._directoryInfo.FullPath, destinationName);

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
                    //return new StoreItemResult(result, new MailruStoreItem(LockingManager, new FileInfo(destinationPath), IsWritable));
                    return new StoreItemResult(result, new MailruStoreItem(LockingManager, new MailRuCloudApi.File(), IsWritable));
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
            Debug.Assert(item is MailruStoreCollection);

            // Collections will never be moved, but always be created
            // (we always move the individual items to make sure locking is checked properly)
            throw new InvalidOperationException("Collections should never be moved directly.");
        }

        public Task<DavStatusCode> DeleteItemAsync(string name, IHttpContext httpContext)
        {
            // Return error
            if (!IsWritable)
                return Task.FromResult(DavStatusCode.PreconditionFailed);

            //Cloud._cloud.Remove()

            // Determine the full path
            var fullPath = Path.Combine(_directoryInfo.FullPath, name);
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
                s_log.Log(LogLevel.Error, $"Unable to delete '{fullPath}' directory.", exc);
                return Task.FromResult(DavStatusCode.InternalServerError);
            }
        }

        public bool AllowInfiniteDepthProperties => false;

        public override int GetHashCode()
        {
            return _directoryInfo.FullPath.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            var storeCollection = obj as MailruStoreCollection;
            if (storeCollection == null)
                return false;
            return storeCollection._directoryInfo.FullPath.Equals(_directoryInfo.FullPath, StringComparison.CurrentCultureIgnoreCase);
        }
    }
}