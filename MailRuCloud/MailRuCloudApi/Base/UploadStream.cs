using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using YaR.MailRuCloud.Api.Base.Requests;
using YaR.MailRuCloud.Api.Extensions;

namespace YaR.MailRuCloud.Api.Base
{
    internal class UploadStream : Stream
    {
        private static readonly log4net.ILog Logger = log4net.LogManager.GetLogger(typeof(UploadStream));

        public UploadStream(string destinationPath, MailRuCloud cloud, long size)
        {
            _cloud = cloud;

            _file = new File(destinationPath, size, null);
            _shard = _cloud.CloudApi.Account.GetShardInfo(ShardType.Upload).Result;

            Initialize();
        }

        private void Initialize()
        {
            _requestTask = Task.Run(async () =>
            {
                try
                {
                    var boundary = new UploadMultipartBoundary(_file);
                    var url = new Uri($"{_shard.Url}?cloud_domain=2&{_cloud.CloudApi.Account.Credentials.Login}");

                    _request = (HttpWebRequest) WebRequest.Create(url.OriginalString);
                    _request.Proxy = _cloud.CloudApi.Account.Proxy;
                    _request.CookieContainer = _cloud.CloudApi.Account.Cookies;
                    _request.Method = "POST";
                    _request.ContentLength = _file.OriginalSize + boundary.Start.LongLength + boundary.End.LongLength;
                    _request.Referer = $"{ConstSettings.CloudDomain}/home/{Uri.EscapeDataString(_file.Path)}";
                    _request.Headers.Add("Origin", ConstSettings.CloudDomain);
                    _request.Host = url.Host;
                    _request.ContentType = $"multipart/form-data; boundary=----{boundary.Guid}";
                    _request.Accept = "*/*";
                    _request.UserAgent = ConstSettings.UserAgent;
                    _request.AllowWriteStreamBuffering = false;
                    Logger.Debug($"HTTP:{_request.Method}:{_request.RequestUri.AbsoluteUri}");

                    
                    var requeststream = await _request.GetRequestStreamAsync();
                    {
                        await requeststream.WriteAsync(boundary.Start, 0, boundary.Start.Length);
                        await _ringBuffer.CopyToAsync(requeststream);
                        await requeststream.WriteAsync(boundary.End, 0, boundary.End.Length);
                    }

                    var response = _request.GetResponse();
                    return (HttpWebResponse)response;
                }
                catch (Exception e)
                {
                    Logger.Error("Upload request failed", e);
                    throw;
                }
            });
        }

        public bool CheckHashes { get; set; } = true;

        public override void Write(byte[] buffer, int offset, int count)
        {
            if (CheckHashes)
                _sha1.Append(buffer, offset, count);

            _ringBuffer.Write(buffer, offset, count);
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (!disposing) return;

            _ringBuffer.Flush();

            using (var response = _requestTask.Result)
            {
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    var resp = response.ReadAsText(_cloud.CloudApi.CancelToken)
                        .ToUploadPathResult();

                    _file.OriginalSize = resp.Size;
                    _file.Hash = resp.Hash;

                    if (CheckHashes && _sha1.HashString != resp.Hash)
                        throw new HashMatchException(_sha1.HashString, resp.Hash);

                    var res = _cloud.AddFileInCloud(_file, ConflictResolver.Rewrite).Result;
                }
                else
                    throw new Exception("Cannot upload file, status " + response.StatusCode);
            }

        }


        private readonly MailRuCloud _cloud;
        private readonly File _file;
        private readonly ShardInfo _shard;

        private readonly MailRuSha1Hash _sha1 = new MailRuSha1Hash();
        private HttpWebRequest _request;
        private Task<HttpWebResponse> _requestTask;
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
