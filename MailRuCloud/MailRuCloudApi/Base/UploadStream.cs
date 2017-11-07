using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using YaR.MailRuCloud.Api.Base.Requests;

namespace YaR.MailRuCloud.Api.Base
{
    internal class UploadStream : Stream
    {
        private static readonly log4net.ILog Logger = log4net.LogManager.GetLogger(typeof(UploadStream));

        private readonly CloudApi _cloud;
        private readonly File _file;
        private readonly ShardInfo _shard;

        public UploadStream(string destinationPath, CloudApi cloud, long size)
        {
            _cloud = cloud;

            _file = new File(destinationPath, size, null);
            _shard = _cloud.Account.GetShardInfo(ShardType.Upload).Result;

            Initialize();
        }

        private HttpWebRequest _request;
        private byte[] _endBoundaryRequest;



        private void Initialize()
        {
            //// Boundary request building.
            var boundary = Guid.NewGuid();
            var boundaryBuilder = new StringBuilder();
            boundaryBuilder.AppendFormat("------{0}\r\n", boundary);
            boundaryBuilder.AppendFormat("Content-Disposition: form-data; name=\"file\"; filename=\"{0}\"\r\n", Uri.EscapeDataString(_file.Name));
            boundaryBuilder.AppendFormat("Content-Type: {0}\r\n\r\n", ConstSettings.GetContentType(_file.Extension));

            var endBoundaryBuilder = new StringBuilder();
            endBoundaryBuilder.AppendFormat("\r\n------{0}--\r\n", boundary);

            _endBoundaryRequest = Encoding.UTF8.GetBytes(endBoundaryBuilder.ToString());
            var boundaryRequest = Encoding.UTF8.GetBytes(boundaryBuilder.ToString());

            var url = new Uri($"{_shard.Url}?cloud_domain=2&{_cloud.Account.LoginName}");
            _request = (HttpWebRequest)WebRequest.Create(url.OriginalString);
            _request.Proxy = _cloud.Account.Proxy;
            _request.CookieContainer = _cloud.Account.Cookies;
            _request.Method = "POST";

            _request.ContentLength = _file.Size + boundaryRequest.LongLength + _endBoundaryRequest.LongLength;

            _request.Referer = $"{ConstSettings.CloudDomain}/home/{Uri.EscapeDataString(_file.Path)}";
            _request.Headers.Add("Origin", ConstSettings.CloudDomain);
            _request.Host = url.Host;
            _request.ContentType = $"multipart/form-data; boundary=----{boundary}";
            _request.Accept = "*/*";
            _request.UserAgent = ConstSettings.UserAgent;
            _request.AllowWriteStreamBuffering = false;

            _request.KeepAlive = false;
            _request.Timeout = Timeout.Infinite;
            _request.ProtocolVersion = HttpVersion.Version10;



            _requestStream = _request.GetRequestStream();
            Logger.Debug($"HTTP:{_request.Method}:{_request.RequestUri.AbsoluteUri}");

            _requestStream.Write(boundaryRequest, 0, boundaryRequest.Length);
        }

        private Stream _requestStream;

        public override void Write(byte[] buffer, int offset, int count)
        {
            _requestStream.Write(buffer, offset, count);
        }

        public override Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            return _requestStream.WriteAsync(buffer, offset, count, cancellationToken);
        }

        protected override void Dispose(bool disposing)
        {

            if (_requestStream != null)
            {
                if (disposing)
                {
                    _requestStream.Write(_endBoundaryRequest, 0, _endBoundaryRequest.Length);
                    _requestStream.Close();

                    using (var response = (HttpWebResponse)_request.GetResponse())
                    {
                        if (response.StatusCode == HttpStatusCode.OK)
                        {
                            var resp = ReadResponseAsText(response, _cloud.CancelToken).Split(';');
                            var hashResult = resp[0];
                            var sizeResult = long.Parse(resp[1].Trim('\r', '\n', ' '));

                            _file.Hash = hashResult;
                            _file.Size = sizeResult;

                            var res = AddFileInCloud(_file).Result;
                        }
                    }
                }
            }
            
        }


        private async Task<bool> AddFileInCloud(File fileInfo, ResolveFileConflictMethod conflict = ResolveFileConflictMethod.Rewrite)
        {
            await new CreateFileRequest(_cloud, fileInfo.FullPath, fileInfo.Hash, fileInfo.Size, conflict)
                .MakeRequestAsync();

            return true;
        }


        private static string ReadResponseAsText(WebResponse resp, CancellationTokenSource cancelToken)
        {
            using (var stream = new MemoryStream())
            {
                try
                {
                    ReadResponseAsByte(resp, cancelToken.Token, stream);
                    return Encoding.UTF8.GetString(stream.ToArray());
                }
                catch
                {
                    //// Cancellation token.
                    return "7035ba55-7d63-4349-9f73-c454529d4b2e";
                }
            }
        }


        private static void ReadResponseAsByte(WebResponse resp, CancellationToken token, Stream outputStream = null)
        {
            using (Stream responseStream = resp.GetResponseStream())
            {
                var buffer = new byte[65536];
                int bytesRead;

                while (responseStream != null && (bytesRead = responseStream.Read(buffer, 0, buffer.Length)) > 0)
                {
                    token.ThrowIfCancellationRequested();
                    outputStream?.Write(buffer, 0, bytesRead);
                }
            }
        }

        public override void Flush()
        {
            _requestStream.Flush();
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            return _requestStream.Seek(offset, origin);
        }

        public override void SetLength(long value)
        {
            _requestStream.SetLength(value);
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            return _requestStream.Read(buffer, offset, count);
        }
        public override Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            return _requestStream.ReadAsync(buffer, offset, count, cancellationToken);
        }

        public override bool CanRead => _requestStream.CanRead;
        public override bool CanSeek => _requestStream.CanSeek;
        public override bool CanWrite => _requestStream.CanWrite;
        public override long Length => _requestStream.Length;
        public override long Position { get => _requestStream.Position; set => _requestStream.Position = value; }

        public override Task CopyToAsync(Stream destination, int bufferSize, CancellationToken cancellationToken)
        {
            return _requestStream.CopyToAsync(destination, bufferSize, cancellationToken);
        }


    }
}
