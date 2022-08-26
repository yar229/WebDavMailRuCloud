using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using NWebDav.Server;
using NWebDav.Server.Helpers;
using NWebDav.Server.Http;
using NWebDav.Server.Locking;
using NWebDav.Server.Logging;
using NWebDav.Server.Props;
using NWebDav.Server.Stores;
using YaR.Clouds.Base;
using File = YaR.Clouds.Base.File;

namespace YaR.Clouds.WebDavStore.StoreBase
{
    [DebuggerDisplay("{_fileInfo.FullPath}")]
    public class LocalStoreItem : ILocalStoreItem
    {
        private static readonly ILogger Logger = LoggerFactory.Factory.CreateLogger(typeof(LocalStoreItem));


        private readonly File _fileInfo;
        private readonly LocalStore _store;

        public File FileInfo => _fileInfo;
        public IEntry EntryInfo => FileInfo;
        public long Length => _fileInfo.Size;
        public bool IsReadable => true;

        public LocalStoreItem(File fileInfo, bool isWritable, LocalStore store)
        {
            _store = store;

            _fileInfo = fileInfo;

            IsWritable = isWritable;
        }

        //public readonly PropertyManager<LocalStoreItem> DefaultPropertyManager;




        public bool IsWritable { get; }
        public string Name => _fileInfo.Name;
        public string UniqueKey => _fileInfo.FullPath;
        public string FullPath => _fileInfo.FullPath;

        public IPropertyManager PropertyManager => _store.ItemPropertyManager;
        public ILockingManager LockingManager => _store.LockingManager;


        private Stream OpenReadStream(Cloud cloud, long? start, long? end)
        {
            Stream stream = cloud.GetFileDownloadStream(_fileInfo, start, end).Result;
            return stream;
        }


        public Task<Stream> GetReadableStreamAsync(IHttpContext httpContext) //=> 
        {
            var cloud = CloudManager.Instance((HttpListenerBasicIdentity)httpContext.Session.Principal.Identity);
            var range = httpContext.Request.GetRange();
            return Task.FromResult(OpenReadStream(cloud, range?.Start, range?.End));
        }

        public async Task<DavStatusCode> UploadFromStreamAsync(IHttpContext httpContext, Stream inputStream)
        {
            if (!IsWritable)
                return DavStatusCode.Conflict;


            // dirty hack! HIGH MEMORY CONSUME
            // mail.ru needs size of file, but some clients does not send it
            // so we'll cache file in memory
            // TODO: rewrite
            if (httpContext.Request.GetHeaderValue("Transfer-Encoding") == "chunked" && _fileInfo.Size == 0)
            {
                Logger.Log(LogLevel.Warning, () => "Client does not send file size, caching in memory!");
                var memStream = new MemoryStream();
                await inputStream.CopyToAsync(memStream).ConfigureAwait(false);

                _fileInfo.OriginalSize = new FileSize(memStream.Length);

                using (var outputStream = IsWritable
                    ? await CloudManager.Instance(httpContext.Session.Principal.Identity).GetFileUploadStream(_fileInfo.FullPath, _fileInfo.Size, null, null).ConfigureAwait(false)
                    : null)
                {
                    memStream.Seek(0, SeekOrigin.Begin);
                    await memStream.CopyToAsync(outputStream).ConfigureAwait(false);
                }
                return DavStatusCode.Ok;
            }

            // Copy the stream
            try
            {
                var cts = new CancellationTokenSource();

                // После, собственно, закачки файла, сервер может, например, считать хэш файла (Яндекс.Диск) и это может быть долго
                // Чтобы клиент не отваливался по таймауту, пишем в ответ понемножку пробелы
                void StreamCopiedAction()
                {
                    var _ = Task.Run(() =>
                    {
                        while (!cts.IsCancellationRequested)
                        {
                            Thread.Sleep(7_000);
                            if (cts.IsCancellationRequested) break;

                            httpContext.Response.Stream.WriteByte((byte) ' ');
                            httpContext.Response.Stream.Flush();

                            Logger.Log(LogLevel.Debug, "Waiting for server processing file...");
                        }
                    }, cts.Token);
                }

                void ServerProcessFinishedAction()
                {
                    //Thread.Sleep(10_000);
                    cts.Cancel();
                    Logger.Log(LogLevel.Debug, "Server finished file processing");
                }


                // Copy the information to the destination stream
                using (var outputStream = IsWritable 
                    ? await CloudManager.Instance(httpContext.Session.Principal.Identity).GetFileUploadStream(_fileInfo.FullPath, _fileInfo.Size, StreamCopiedAction, ServerProcessFinishedAction).ConfigureAwait(false)
                    : null)
                {
#if NET48
                    await inputStream.CopyToAsync(outputStream).ConfigureAwait(false);
#else
                    await inputStream.CopyToAsync(outputStream, cts.Token).ConfigureAwait(false);
#endif
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
                if (destination is LocalStoreCollection collection)
                {
                    if (!collection.IsWritable)
                        return new StoreItemResult(DavStatusCode.PreconditionFailed);

                    //var destinationPath = WebDavPath.Combine(collection.FullPath, name);

                    // check if the file already exists??

                    await CloudManager.Instance(httpContext.Session.Principal.Identity).Copy(_fileInfo, collection.FullPath);

                    return new StoreItemResult(DavStatusCode.Created);
                }


                // Create the item in the destination collection
                var result = await destination.CreateItemAsync(name, overwrite, httpContext).ConfigureAwait(false);

                if (result.Item == null) 
                    return new StoreItemResult(result.Result, result.Item);

                using (var sourceStream = await GetReadableStreamAsync(httpContext).ConfigureAwait(false))
                {
                    var copyResult = await result.Item.UploadFromStreamAsync(httpContext, sourceStream).ConfigureAwait(false);
                    if (copyResult != DavStatusCode.Ok)
                        return new StoreItemResult(copyResult, result.Item);
                }

                return new StoreItemResult(result.Result, result.Item);
            }
            catch (IOException ioException) when (ioException.IsDiskFull())
            {
                Logger.Log(LogLevel.Error, () => "Out of disk space while copying data.", ioException);
                return new StoreItemResult(DavStatusCode.InsufficientStorage);
            }
            catch (Exception exc)
            {
                Logger.Log(LogLevel.Error, () => "Unexpected exception while copying data.", exc);
                return new StoreItemResult(DavStatusCode.InternalServerError);
            }
        }

        public override int GetHashCode()
        {
            return _fileInfo.FullPath.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (obj is not LocalStoreItem storeItem)
                return false;
            return storeItem._fileInfo.FullPath.Equals(_fileInfo.FullPath, StringComparison.CurrentCultureIgnoreCase);
        }

        public string DetermineContentType()
        {
            return MimeTypeHelper.GetMimeTypeByExtension(_fileInfo.Extension);
        }

        public string CalculateEtag()
        {
            string h = _fileInfo.FullPath + _fileInfo.Size;
            var hash = SHA256.Create().ComputeHash(GetBytes(h));
            return BitConverter.ToString(hash).Replace("-", string.Empty);
        }


        private static byte[] GetBytes(string str)
        {
            byte[] bytes = new byte[str.Length * sizeof(char)];
            Buffer.BlockCopy(str.ToCharArray(), 0, bytes, 0, bytes.Length);
            return bytes;
        }
    }



}
