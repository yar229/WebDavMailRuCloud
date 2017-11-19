using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
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

            Initialize();
        }

        private void Initialize()
        {
            _requestTask = Task.Run(() =>
            {
                try
                {
                    //var boundary = new UploadMultipartBoundary(_file);
                    var shard = _cloud.CloudApi.Account.GetShardInfo(ShardType.Upload).Result;
                    var url = new Uri($"{shard.Url}?cloud_domain=2&{_cloud.CloudApi.Account.Credentials.Login}");

                    var config = new HttpClientHandler
                    {
                        UseProxy = true,
                        Proxy = _cloud.CloudApi.Account.Proxy,
                        CookieContainer = _cloud.CloudApi.Account.Cookies,
                        UseCookies = true,
                        AllowAutoRedirect = true,
                        //MaxRequestContentBufferSize = 65536
                    };


                    _client = new HttpClient(config);
                    _client.Timeout = Timeout.InfiniteTimeSpan;

                    _request = new HttpRequestMessage()
                    {
                        RequestUri = url,
                        Method = HttpMethod.Post,
                    };
                    
                    _request.Headers.Add("Referer", $"{ConstSettings.CloudDomain}/home/{Uri.EscapeDataString(_file.Path)}");
                    _request.Headers.Add("Origin", ConstSettings.CloudDomain);
                    _request.Headers.Add("Host", url.Host);
                    _request.Headers.Add("Accept", "*/*");
                    //request.Headers.Add("Content-Length", _file.OriginalSize);
                    
                    _request.Headers.TryAddWithoutValidation("User-Agent", ConstSettings.UserAgent);
                    
                    var guid = Guid.NewGuid();
                    var content = new MultipartFormDataContent($"----{guid}");
                    var boundaryValue = content.Headers.ContentType.Parameters.First(p => p.Name == "boundary");
                    boundaryValue.Value = boundaryValue.Value.Replace("\"", String.Empty);
                    //content.Headers.Add("Content-Disposition", $"form-data; name=\"file\"; filename=\"{_file.Name}\"");
                    //content.Headers.TryAddWithoutValidation("Content-Type", "application/octet-stream");
                    //content.Headers.Add("Content-Type", "application/octet-stream");


                    //var streamContent = new StreamContent(_ringBuffer);
                    //streamContent.Headers.Add("Content-Disposition", $"form-data; name=\"file\"; filename=\"{_file.Name}\"");
                    //streamContent.Headers.Add("Content-Type", "application/octet-stream");
                    //content.Add(streamContent);

                    //var pushContent = new StreamContent(_ringBuffer);
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
                    _pushContent.Headers.Add("Content-Disposition", $"form-data; name=\"file\"; filename=\"{_file.Name}\"");
                    content.Add(_pushContent);


                    _request.Content = content;
                    _request.Content.Headers.ContentLength = _file.OriginalSize + 192 + Encoding.UTF8.GetBytes(_file.Name).Length;

                    _responseMessage = _client.SendAsync(_request).Result;

                    var zz = 1;


                    //_request = (HttpWebRequest)WebRequest.Create(url.OriginalString);
                    //_request.Proxy = _cloud.CloudApi.Account.Proxy;
                    //_request.CookieContainer = _cloud.CloudApi.Account.Cookies;
                    //_request.Method = "POST";
                    //_request.ContentLength = _file.OriginalSize + boundary.Start.LongLength + boundary.End.LongLength;
                    //_request.Referer = $"{ConstSettings.CloudDomain}/home/{Uri.EscapeDataString(_file.Path)}";
                    //_request.Headers.Add("Origin", ConstSettings.CloudDomain);
                    //_request.Host = url.Host;
                    //_request.ContentType = $"multipart/form-data; boundary=----{boundary.Guid}";
                    //_request.Accept = "*/*";
                    //_request.UserAgent = ConstSettings.UserAgent;
                    //_request.AllowWriteStreamBuffering = false;
                    //Logger.Debug($"HTTP:{_request.Method}:{_request.RequestUri.AbsoluteUri}");

                    //using (var requeststream = await _request.GetRequestStreamAsync())
                    //{
                    //    await requeststream.WriteAsync(boundary.Start, 0, boundary.Start.Length);
                    //    await _ringBuffer.CopyToAsync(requeststream);
                    //    await requeststream.WriteAsync(boundary.End, 0, boundary.End.Length);
                    //}

                    //var response = _request.GetResponse();
                    //return (HttpWebResponse)response;
                }
                catch (Exception e)
                {
                    Logger.Error("Upload request failed", e);
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
            if (CheckHashes)
                _sha1.Append(buffer, offset, count);

            _ringBuffer.Write(buffer, offset, count);
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (!disposing) return;

            _ringBuffer.Flush();

            _requestTask.GetAwaiter().GetResult();

            //using (var response = _request. )
            {
                if (_responseMessage.StatusCode != HttpStatusCode.OK)
                    throw new Exception("Cannot upload file, status " + _responseMessage.StatusCode);

                var resb = _responseMessage.Content.ReadAsByteArrayAsync().Result;
                var ures = _responseMessage.Content.ReadAsStringAsync().Result
                    .ToUploadPathResult();

                _file.OriginalSize = ures.Size;
                _file.Hash = ures.Hash;

                if (CheckHashes && _sha1.HashString != ures.Hash)
                    throw new HashMatchException(_sha1.HashString, ures.Hash);

                var z = _cloud.AddFileInCloud(_file, ConflictResolver.Rewrite)
                    .Result;
                //.ThrowIf(r => r.status != 200, r => new Exception("Cannot add file, status " + r.status));
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
