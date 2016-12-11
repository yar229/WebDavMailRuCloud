using System;
using System.Diagnostics;
using System.IO;
using System.Security.Cryptography;
using System.Threading.Tasks;
using System.Xml.Linq;
using NWebDav.Server;
using NWebDav.Server.Helpers;
using NWebDav.Server.Http;
using NWebDav.Server.Locking;
using NWebDav.Server.Logging;
using NWebDav.Server.Props;
using NWebDav.Server.Stores;
using WebDavMailRuCloudStore;
using YaR.WebDavMailRu.CloudStore.DavCustomProperty;

namespace YaR.WebDavMailRu.CloudStore.Mailru.StoreBase
{
    [DebuggerDisplay("{_fileInfo.FullPath}")]
    public sealed class MailruStoreItem : IMailruStoreItem
    {
        private static readonly ILogger SLog = LoggerFactory.Factory.CreateLogger(typeof(MailruStoreItem));
        private readonly MailRuCloudApi.File _fileInfo;

        public MailRuCloudApi.File FileInfo => _fileInfo;

        private MailruStoreItem()
        {
            
        }

        public MailruStoreItem(ILockingManager lockingManager, MailRuCloudApi.File fileInfo, bool isWritable)
        {
            LockingManager = lockingManager;
            _fileInfo = fileInfo;
            IsWritable = isWritable;
        }

        public static PropertyManager<MailruStoreItem> DefaultPropertyManager { get; } = new PropertyManager<MailruStoreItem>(new DavProperty<MailruStoreItem>[]
        {
            // RFC-2518 properties
            new DavCreationDate<MailruStoreItem>
            {
                Getter = (context, item) => item._fileInfo.CreationTimeUtc,
                Setter = (context, item, value) =>
                {
                    item._fileInfo.CreationTimeUtc = value;
                    return DavStatusCode.Ok;
                }
            },
            new DavDisplayName<MailruStoreItem>
            {
                Getter = (context, item) => item._fileInfo.Name
            },
            new DavGetContentLength<MailruStoreItem>
            {
                Getter = (context, item) => item._fileInfo.Size?.DefaultValue ?? 0
            },
            new DavGetContentType<MailruStoreItem>
            {
                Getter = (context, item) => item.DetermineContentType()
            },
            new DavGetEtag<MailruStoreItem>
            {
                // Calculating the Etag is an expensive operation,
                // because we need to scan the entire file.
                IsExpensive = true,
                Getter = (context, item) => item.CalculateEtag()
            },
            new DavGetLastModified<MailruStoreItem>
            {
                Getter = (context, item) => item._fileInfo.LastWriteTimeUtc,
                Setter = (context, item, value) =>
                {
                    item._fileInfo.LastWriteTimeUtc = value;
                    return DavStatusCode.Ok;
                }
            },
            new DavGetResourceType<MailruStoreItem>
            {
                Getter = (context, item) => null
            },

            // Default locking property handling via the LockingManager
            new DavLockDiscoveryDefault<MailruStoreItem>(),
            new DavSupportedLockDefault<MailruStoreItem>(),

            // Hopmann/Lippert collection properties
            // (although not a collection, the IsHidden property might be valuable)
            new DavExtCollectionIsHidden<MailruStoreItem>
            {
                Getter = (context, item) => false //(item._fileInfo.Attributes & FileAttributes.Hidden) != 0
            },

            // Win32 extensions
            new Win32CreationTime<MailruStoreItem>
            {
                Getter = (context, item) => item._fileInfo.CreationTimeUtc,
                Setter = (context, item, value) =>
                {
                    item._fileInfo.CreationTimeUtc = value;
                    return DavStatusCode.Ok;
                }
            },
            new Win32LastAccessTime<MailruStoreItem>
            {
                Getter = (context, item) => item._fileInfo.LastAccessTimeUtc,
                Setter = (context, item, value) =>
                {
                    item._fileInfo.LastAccessTimeUtc = value;
                    return DavStatusCode.Ok;
                }
            },
            new Win32LastModifiedTime<MailruStoreItem>
            {
                Getter = (context, item) => item._fileInfo.LastWriteTimeUtc,
                Setter = (context, item, value) =>
                {
                    item._fileInfo.LastWriteTimeUtc = value;
                    return DavStatusCode.Ok;
                }
            },
            new Win32FileAttributes<MailruStoreItem>
            {
                Getter = (context, item) => FileAttributes.Normal, //item._fileInfo.Attributes,
                Setter = (context, item, value) => DavStatusCode.Ok
            },
            new DavSharedLink<MailruStoreItem>
            {
                Getter = (context, item) => item._fileInfo.PublicLink,
                Setter = (context, item, value) => DavStatusCode.Ok
            }
        });





        public bool IsWritable { get; }
        public string Name => _fileInfo.Name;
        public string UniqueKey => _fileInfo.FullPath;
        public string FullPath => _fileInfo.FullPath;
        public IPropertyManager PropertyManager => DefaultPropertyManager;
        public ILockingManager LockingManager { get; }


        private Stream OpenReadStream(long? start, long? end)
        {

            Stream stream = Cloud.Instance.GetFileStream(_fileInfo, start, end).Result;
            return stream;
        }


        public Task<Stream> GetReadableStreamAsync(IHttpContext httpContext) //=> 
        {
            var range = httpContext.Request.GetRange();
            return Task.FromResult(OpenReadStream(range?.Start, range?.End));
        }

        //{
        //    return new Task<Stream>(OpenReadStream);
        //}

        public async Task<DavStatusCode> UploadFromStreamAsync(IHttpContext httpContext, Stream inputStream)
        {
            // Check if the item is writable
            if (!IsWritable)
                return DavStatusCode.Conflict;

            long allowedSize = Cloud.Instance.Account.Info.FileSizeLimit - _fileInfo.Name.BytesCount();
            if (_fileInfo.Size.DefaultValue > allowedSize)
            {
                //inputStream.Close();
                return DavStatusCode.PreconditionFailed;
            }

            // Copy the stream
            try
            {
                // Copy the information to the destination stream
                using (var outputStream = IsWritable ? Cloud.Instance.GetUploadStream(_fileInfo.FullPath, ".bin", _fileInfo.Size.DefaultValue) : null)  //GetWritableStream(httpContext))
                {
                    await inputStream.CopyToAsync(outputStream).ConfigureAwait(false);
                }
                return DavStatusCode.Ok;
            }
            catch (IOException ioException) when (ioException.IsDiskFull())
            {
                return DavStatusCode.InsufficientStorage;
            }

        }

        public async Task<StoreItemResult> CopyAsync(IStoreCollection destination, string name, bool overwrite, IHttpContext httpContext)
        {
            try
            {
                // If the destination is also a disk-store, then we can use the FileCopy API
                // (it's probably a bit more efficient than copying in C#)
                var diskCollection = destination as MailruStoreCollection;
                if (diskCollection != null)
                {
                    // Check if the collection is writable
                    //if (!diskCollection.IsWritable)
                    //    return new StoreItemResult(DavStatusCode.PreconditionFailed);

                    var destinationPath = Path.Combine(diskCollection.FullPath, name);

                    // Check if the file already exists
                    //var fileExists = File.Exists(destinationPath);
                    //if (fileExists && !overwrite)
                    //    return new StoreItemResult(DavStatusCode.PreconditionFailed);

                    // Copy the file
                    //File.Copy(_fileInfo.FullName, destinationPath, true);
                    Cloud.Instance.Copy(_fileInfo, destinationPath).Wait();

                    // Return the appropriate status
                    //return new StoreItemResult(fileExists ? DavStatusCode.NoContent : DavStatusCode.Created);
                    return new StoreItemResult(DavStatusCode.Created);
                }
                else
                {
                    // Create the item in the destination collection
                    var result = await destination.CreateItemAsync(name, overwrite, httpContext).ConfigureAwait(false);


                    if (result.Item != null)
                    {
                        using (var sourceStream = await GetReadableStreamAsync(httpContext).ConfigureAwait(false))
                        {
                            var copyResult = await result.Item.UploadFromStreamAsync(httpContext, sourceStream).ConfigureAwait(false);
                            if (copyResult != DavStatusCode.Ok)
                                return new StoreItemResult(copyResult, result.Item);
                        }
                    }

                    // Check if the item could be created
                    ////if (result.Item != null)
                    ////{
                    //    using (var destinationStream = result.Item.GetWritableStream(httpContext))
                    //    using (var sourceStream = GetReadableStream(httpContext))
                    //    {
                    //        await sourceStream.CopyToAsync(destinationStream).ConfigureAwait(false);
                    //    }
                    ////}

                    // Return result
                    return new StoreItemResult(result.Result, result.Item);
                }
            }
            catch (IOException ioException) when (ioException.IsDiskFull())
            {
                SLog.Log(LogLevel.Error, () => "Out of disk space while copying data.", ioException);
                return new StoreItemResult(DavStatusCode.InsufficientStorage);
            }
            catch (Exception exc)
            {
                SLog.Log(LogLevel.Error, () => "Unexpected exception while copying data.", exc);
                return new StoreItemResult(DavStatusCode.InternalServerError);
            }
        }

        public override int GetHashCode()
        {
            return _fileInfo.FullPath.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            var storeItem = obj as MailruStoreItem;
            if (storeItem == null)
                return false;
            return storeItem._fileInfo.FullPath.Equals(_fileInfo.FullPath, StringComparison.CurrentCultureIgnoreCase);
        }

        private string DetermineContentType()
        {
            return MimeTypeHelper.GetMimeType(_fileInfo.Name);
        }

        private string CalculateEtag()
        {
            string h = _fileInfo.FullPath + _fileInfo.Size;
            var hash = SHA256.Create().ComputeHash(GetBytes(h));
            return BitConverter.ToString(hash).Replace("-", string.Empty);
        }


        static byte[] GetBytes(string str)
        {
            byte[] bytes = new byte[str.Length * sizeof(char)];
            Buffer.BlockCopy(str.ToCharArray(), 0, bytes, 0, bytes.Length);
            return bytes;
        }
    }
}
