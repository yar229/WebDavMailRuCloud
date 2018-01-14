using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using YaR.MailRuCloud.Api.Base.Requests;
using YaR.MailRuCloud.Api.Base.Requests.Types;
using YaR.MailRuCloud.Api.Extensions;

namespace YaR.MailRuCloud.Api.Base.Streams
{
    /// <summary>
    /// Upload stream based on HttpClient
    /// </summary>
    /// <remarks>Suitable for .NET Core, on .NET desktop POST requests does not return response content.</remarks>
    abstract class UploadStreamHttpClient : Stream
    {
        private static readonly log4net.ILog Logger = log4net.LogManager.GetLogger(typeof(UploadStream));

        protected UploadStreamHttpClient(string destinationPath, MailRuCloud cloud, long size)
        {
            _cloud = cloud;
            _file = new File(destinationPath, size, null);

            Initialize();
        }

        private void Initialize()
        {
            _requestTask = Task.Run(() =>
            {
                try
                {
                    if (_file.OriginalSize <= MailRuSha1Hash.Length) // do not send upload request if file content fits to hash
                    {
                        using (var ms = new MemoryStream())
                        {
                            _ringBuffer.CopyTo(ms);
                        }
                        return;
                    }



                    var shard = _cloud.Account.RequestRepo.GetShardInfo(ShardType.Upload).Result;
                    var url = new Uri($"{shard.Url}?token={_cloud.Account.RequestRepo.Authent.AccessToken}");

                    var config = new HttpClientHandler
                    {
                        UseProxy = true,
                        Proxy = _cloud.Account.RequestRepo.HttpSettings.Proxy,
                        CookieContainer = _cloud.Account.RequestRepo.Authent.Cookies,
                        UseCookies = true,
                        AllowAutoRedirect = true,
                    };

                    _client = new HttpClient(config) {Timeout = Timeout.InfiniteTimeSpan};

                    _request = new HttpRequestMessage
                    {
                        RequestUri = url,
                        Method = HttpMethod.Put
                    };

                    _request.Headers.Add("Accept", "*/*");
                    _request.Headers.TryAddWithoutValidation("User-Agent", _cloud.Account.RequestRepo.HttpSettings.UserAgent);

                    _pushContent = new PushStreamContent((stream, httpContent, arg3) =>
                    {
                        try
                        {
                            _ringBuffer.CopyTo(stream);
                            stream.Close();

                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e);
                            throw;
                        }
                    });

                    _request.Content = _pushContent;
                    _request.Content.Headers.ContentLength = _file.OriginalSize;

                    _responseMessage = _client.SendAsync(_request).Result;
                }
                catch (Exception e)
                {
                    Logger.Error($"Uploading to {_file.FullPath} failed with {e.Message}"); //TODO remove duplicate exception catch?
                    throw;
                }
            });
        }

        private PushStreamContent _pushContent;
        private HttpResponseMessage _responseMessage;
        private HttpClient _client;
        private HttpRequestMessage _request;

        public bool CheckHashes { get; set; } = true;

        public override void Write(byte[] buffer, int offset, int count)
        {
            if (CheckHashes || _file.OriginalSize <= MailRuSha1Hash.Length)
                _sha1.Append(buffer, offset, count);

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

                if (null != _responseMessage) // file length > hash length
                {
                    if (_responseMessage.StatusCode != HttpStatusCode.Created &&
                        _responseMessage.StatusCode != HttpStatusCode.OK)
                        throw new Exception("Cannot upload file, status " + _responseMessage.StatusCode);

                    var ures = _responseMessage.Content.ReadAsStringAsync().Result
                        .ToUploadPathResult();

                    if (ures.Size > 0 && _file.OriginalSize != ures.Size)
                        throw new Exception("Local and remote file size does not match");
                    _file.Hash = ures.Hash;

                    if (CheckHashes && _sha1.HashString != ures.Hash)
                        throw new HashMatchException(_sha1.HashString, ures.Hash);
                }
                else
                {
                    _file.Hash = _sha1.HashString;
                }

                _cloud.AddFileInCloud(_file, ConflictResolver.Rewrite)
                    .Result
                    .ThrowIf(r => !r.Success, r => new Exception($"Cannot add file {_file.FullPath}"));
            }
            finally 
            {
                _ringBuffer?.Dispose();
                _sha1?.Dispose();
            }
        }

        private readonly MailRuCloud _cloud;
        private readonly File _file;

        private readonly MailRuSha1Hash _sha1 = new MailRuSha1Hash();
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