using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using YaR.Clouds.Base.Repos;
using YaR.Clouds.Base.Requests.Types;
using YaR.Clouds.Extensions;

namespace YaR.Clouds.Base.Streams
{
    /// <summary>
    /// Upload stream based on HttpClient
    /// </summary>
    /// <remarks>Suitable for .NET Core, on .NET desktop POST requests does not return response content.</remarks>
    abstract class UploadStreamHttpClient : Stream
    {
        private static readonly log4net.ILog Logger = log4net.LogManager.GetLogger(typeof(UploadStreamHttpClient));

        protected UploadStreamHttpClient(string destinationPath, Cloud cloud, long size)
        {
            _cloud = cloud;
            _file = new File(destinationPath, size, null);
            _cloudFileHasher = Repo.GetHasher();

            Initialize();
        }

        public Action FileStreamSent;
        private void OnFileStreamSent() => FileStreamSent?.Invoke();

        public Action ServerFileProcessed;
        private void OnServerFileProcessed() => ServerFileProcessed?.Invoke();


        private void Initialize()
        {
            _requestTask = Task.Run(Function);
        }

        private void Function()
        {
            try
            {
                if (Repo.SupportsAddSmallFileByHash && _file.OriginalSize <= _cloudFileHasher.Length) // do not send upload request if file content fits to hash
                {
                        _ringBuffer.CopyTo(Stream.Null);
                        _file.Hash = _cloudFileHasher?.HashString;
                }
                else if (Repo.SupportsDeduplicate && _cloud.Settings.UseDeduplicate && !_file.ServiceInfo.IsCrypted)
                {
                    var cache = new CacheStream(_file, _ringBuffer, null);
                    if (cache.Process())
                    {
                        _file.Hash = _cloudFileHasher.HashString;
                        bool added = _cloud.AddFileInCloud(_file, ConflictResolver.Rewrite)
                            .Result.Success;

                        if (!added)
                        {
                            FullUpload(cache.Stream);
                        }

                    }
                }
                else
                {
                    FullUpload(_ringBuffer);
                }

                _cloud.AddFileInCloud(_file, ConflictResolver.Rewrite)
                    .Result
                    .ThrowIf(r => !r.Success, r => new Exception($"Cannot add file {_file.FullPath}"));
            }
            catch (Exception e)
            {
                Logger.Error($"Uploading to {_file.FullPath} failed with {e}"); //TODO remove duplicate exception catch?
                throw;
            }
        }

        private void FullUpload(Stream sourceStream)
        {
            _pushContent = new PushStreamContent((stream, httpContent, arg3) =>
            {
                try
                {
                    sourceStream.CopyTo(stream);
                    stream.Flush();
                    stream.Close();
                    OnFileStreamSent();
                }
                catch (Exception e)
                {
                    Logger.Error($"(inner) Uploading to {_file.FullPath} failed with {e}");
                    throw;
                }
            });

            _client = HttpClientFabric.Instance[_cloud.Account];
            _uploadFileResult = Repo.DoUpload(_client, _pushContent, _file).Result;


            if (_uploadFileResult.HttpStatusCode != HttpStatusCode.Created &&
                _uploadFileResult.HttpStatusCode != HttpStatusCode.OK)
                throw new Exception("Cannot upload file, status " + _uploadFileResult.HttpStatusCode);

            if (_uploadFileResult.Size > 0 && _file.OriginalSize != _uploadFileResult.Size)
                throw new Exception("Local and remote file size does not match");
            _file.Hash = _uploadFileResult.Hash;

            if (CheckHashes && !string.IsNullOrEmpty(_uploadFileResult.Hash) &&
                _cloudFileHasher != null && _cloudFileHasher.HashString != _uploadFileResult.Hash)
                throw new HashMatchException(_cloudFileHasher.HashString, _uploadFileResult.Hash);
        }

        private PushStreamContent _pushContent;
        private HttpClient _client;
        private UploadFileResult _uploadFileResult;

        public bool CheckHashes { get; set; } = true;

        public override void Write(byte[] buffer, int offset, int count)
        {
            if (CheckHashes || 
                (_cloudFileHasher != null && Repo.SupportsAddSmallFileByHash && _file.OriginalSize <= _cloudFileHasher.Length) )
                _cloudFileHasher?.Append(buffer, offset, count);

            _ringBuffer.Write(buffer, offset, count);
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (!disposing) return;

            try
            {
                _ringBuffer.Flush();

                _requestTask.GetAwaiter().GetResult();
                OnServerFileProcessed();


            }
            finally 
            {
                _ringBuffer?.Dispose();
                _cloudFileHasher?.Dispose();
            }
        }

        private readonly Cloud _cloud;
        private readonly File _file;

        private IRequestRepo Repo => _cloud.Account.RequestRepo;
        private readonly ICloudHasher _cloudFileHasher;
        private Task _requestTask;
        private readonly RingBufferedStream _ringBuffer = new RingBufferedStream(65536);

        //===========================================================================================================================

        public override bool CanRead => true;
        public override bool CanSeek => true;
        public override bool CanWrite => true;
        public override long Length => _file.OriginalSize;
        public override long Position { get; set; }

        public override void SetLength(long value)
        {
            _file.OriginalSize = value;
        }

        public override void Flush()
        {
            throw new NotImplementedException();
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotImplementedException();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            throw new NotImplementedException();
        }
    }
}