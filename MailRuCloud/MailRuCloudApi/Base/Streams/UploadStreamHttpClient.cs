using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using YaR.Clouds.Base.Repos;
using YaR.Clouds.Base.Streams.Cache;
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
            _file = new File(destinationPath, size);
            _cloudFileHasher = Repo.GetHasher();

            Initialize();
        }

        public Action FileStreamSent;
        private void OnFileStreamSent() => FileStreamSent?.Invoke();

        public Action ServerFileProcessed;
        private void OnServerFileProcessed() => ServerFileProcessed?.Invoke();


        private void Initialize()
        {
            _uploadTask = Task.Run(Upload);
        }

        private void Upload()
        {
            try
            {
                if (Repo.SupportsAddSmallFileByHash && _file.OriginalSize <= _cloudFileHasher.Length) // do not send upload request if file content fits to hash
                    UploadSmall(_ringBuffer);
                else if (Repo.SupportsDeduplicate && _cloud.Settings.UseDeduplicate) // && !_file.ServiceInfo.IsCrypted) // && !_file.ServiceInfo.SplitInfo.IsPart)
                    UploadCache(_ringBuffer);
                else
                    UploadFull(_ringBuffer);
            }
            catch (Exception e)
            {
                Logger.Error($"Uploading to {_file.FullPath} failed with {e}"); //TODO remove duplicate exception catch?
                throw;
            }
        }

        private void UploadSmall(Stream sourceStream)
        {
            Logger.Debug($"Uploading [small file] {_file.FullPath}");

            sourceStream.CopyTo(Null);
            OnFileStreamSent();

            _file.Hash = _cloudFileHasher?.Hash;

            _cloud.AddFileInCloud(_file, ConflictResolver.Rewrite)
                .Result
                .ThrowIf(r => !r.Success, _ => new Exception($"Cannot add file {_file.FullPath}"));
        }

        private void UploadCache(Stream sourceStream)
        {
            using var cache = new CacheStream(_file, sourceStream, _cloud.Settings.DeduplicateRules);

            if (cache.Process())
            {
                Logger.Debug($"Uploading [{cache.DataCacheName}] {_file.FullPath}");
                
                OnFileStreamSent();

                _file.Hash = _cloudFileHasher.Hash;
                bool added = _cloud.AddFileInCloud(_file, ConflictResolver.Rewrite)
                    .Result
                    .Success;

                if (!added)
                    UploadFull(cache.Stream, false);
            }
            else
            {
                UploadFull(sourceStream);
            }
        }

        private void UploadFull(Stream sourceStream, bool doInvokeFileStreamSent = true)
        {
            Logger.Debug($"Uploading [direct] {_file.FullPath}");

            var pushContent = new PushStreamContent((stream, _, _) =>
            {
                try
                {
                    sourceStream.CopyTo(stream);
                    stream.Flush();
                    stream.Close();
                    if (doInvokeFileStreamSent)
                        OnFileStreamSent();
                }
                catch (Exception e)
                {
                    Logger.Error($"(inner) Uploading to {_file.FullPath} failed with {e}");
                    throw;
                }
            });

            var client = HttpClientFabric.Instance[_cloud.Account];
            var uploadFileResult = Repo.DoUpload(client, pushContent, _file).Result;


            if (uploadFileResult.HttpStatusCode != HttpStatusCode.Created &&
                uploadFileResult.HttpStatusCode != HttpStatusCode.OK)
                throw new Exception("Cannot upload file, status " + uploadFileResult.HttpStatusCode);

            // 2020-10-26 mairu does not return file size now
            //if (uploadFileResult.HasReturnedData && _file.OriginalSize != uploadFileResult.Size)
            //    throw new Exception("Local and remote file size does not match");

            _file.Hash = uploadFileResult.HasReturnedData switch
            {
                true when CheckHashes && null != uploadFileResult.Hash && _cloudFileHasher != null &&
                          _cloudFileHasher.Hash.Hash.Value != uploadFileResult.Hash.Hash.Value => throw
                    new HashMatchException(_cloudFileHasher.Hash.ToString(), uploadFileResult.Hash.ToString()),
                true => uploadFileResult.Hash,
                _ => _file.Hash
            };

            if (uploadFileResult.NeedToAddFile)
                _cloud.AddFileInCloud(_file, ConflictResolver.Rewrite)
                    .Result
                    .ThrowIf(r => !r.Success, _ => new Exception($"Cannot add file {_file.FullPath}"));
        }

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

                _uploadTask.GetAwaiter().GetResult();
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
        private Task _uploadTask;
        private readonly RingBufferedStream _ringBuffer = new(65536);

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