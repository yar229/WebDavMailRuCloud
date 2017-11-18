using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using YaR.MailRuCloud.Api.Base.Requests;
using YaR.MailRuCloud.Api.Extensions;

namespace YaR.MailRuCloud.Api.Base
{

    class Medi : MediaTypeHeaderValue
    {
        protected Medi(MediaTypeHeaderValue source) : base(source)
        {
        }

        public Medi(string mediaType) : base(mediaType)
        {
        }

        public override string ToString()
        {
            return base.ToString();
        }
    }

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

        public bool CheckHashes { get; set; } = true;
        private MailRuSha1Hash _sha1 = new MailRuSha1Hash();


        private byte[] _endBoundaryRequest;

        private void Initialize()
        {
            //// Boundary request building.
            //var boundary = Guid.NewGuid();
            //var boundaryBuilder = new StringBuilder();
            //boundaryBuilder.AppendFormat("------{0}\r\n", boundary);
            //boundaryBuilder.AppendFormat("Content-Disposition: form-data; name=\"file\"; filename=\"{0}\"\r\n", Uri.EscapeDataString(_file.Name));
            //boundaryBuilder.AppendFormat("Content-Type: {0}\r\n\r\n", ConstSettings.GetContentType(_file.Extension));

            //var endBoundaryBuilder = new StringBuilder();
            //endBoundaryBuilder.AppendFormat("\r\n------{0}--\r\n", boundary);

            //_endBoundaryRequest = Encoding.UTF8.GetBytes(endBoundaryBuilder.ToString());
            //var boundaryRequest = Encoding.UTF8.GetBytes(boundaryBuilder.ToString());

            var url = new Uri($"{_shard.Url}?cloud_domain=2&x-email={Uri.EscapeDataString(_cloud.Account.Credentials.Login)}");


            var config = new HttpClientHandler
            {
                UseProxy = true,
                Proxy = _cloud.Account.Proxy, 
                CookieContainer = _cloud.Account.Cookies,
                UseCookies = true
            };

            HttpClient client = new HttpClient(config);
            client.DefaultRequestHeaders.ExpectContinue = false;

            _request = new HttpRequestMessage()
            {
                RequestUri = url,
                Method = HttpMethod.Put,
            };
            _request.Headers.Add("Referer", $"{ConstSettings.CloudDomain}/home{_file.Path}/");
            _request.Headers.Add("Origin", ConstSettings.CloudDomain);
            _request.Headers.Add("Host", url.Host);
            //request.Headers.Add("ContentType", "image/png");
            //request.Headers.Remove("Content-Type");
            //request.Headers.TryAddWithoutValidation("Content-Type", $"multipart / form - data; boundary = ----{ boundary}");
            _request.Headers.Add("Accept", "*/*");
            _request.Headers.Add("UserAgent", ConstSettings.UserAgent);

            _request.Headers.Add("X-Requested-With", "XMLHttpRequest");
            _request.Headers.Add("Accept-Encoding", "gzip, deflate, br");
            _request.Headers.Add("Accept-Language", "ru-RU,ru;q=0.9,en-US;q=0.8,en;q=0.7");
            _request.Headers.Add("Connection", "Keep-Alive");



            var content = new PushStreamContent(async (stream, httpContent, transportContext) =>
            {
                try
                {
                    await _ring.CopyToAsync(stream);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    throw;
                }
                
  
                stream.Close();
            });
            _request.Content = content;

            

            _requesta = client.SendAsync(_request);
            

            //_ring.Write(boundaryRequest, 0, boundaryRequest.Length);
        }


        private readonly byte[] _readbuffer = new byte[65536];
        private RingBufferedStream _ring = new RingBufferedStream(65536);
        private Task<HttpResponseMessage> _requesta;
        private HttpRequestMessage _request;

        public override void Write(byte[] buffer, int offset, int count)
        {
            _ring.Write(buffer, offset, count);
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (!disposing) return;

            //_ring.Write(_endBoundaryRequest, 0, _endBoundaryRequest.Length);

            _ring.Flush();

            using (var response = _requesta.Result)
            {
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    var resp = response.Content.ToString().ToUploadPathResult();

                    _file.OriginalSize = resp.Size;
                    _file.Hash = resp.Hash;

                    if (CheckHashes)
                    {
                        var localHash = _sha1.HashString;
                        if (localHash != resp.Hash)
                            throw new HashMatchException(localHash, resp.Hash);
                    }

                    var res = AddFileInCloud(_file, ConflictResolver.Rewrite).Result;
                }
                else
                {
                    //Logger.Error("zzzzzzzzzzzzzzzzzzzzzzzzzzz" + response.StatusCode);
                    throw new HttpRequestException($"Cannot upload file content to {_request.RequestUri}, result {response.StatusCode}");
                }
            }

            //_stream.Close();
        }


        private async Task<bool> AddFileInCloud(File fileInfo, ConflictResolver? conflict = null)
        {
            await new CreateFileRequest(_cloud, fileInfo.FullPath, fileInfo.Hash, fileInfo.OriginalSize, conflict)
                .MakeRequestAsync();

            return true;
        }


        //private static string ReadResponseAsText(WebResponse resp, CancellationTokenSource cancelToken)
        //{
        //    using (var stream = new MemoryStream())
        //    {
        //        try
        //        {
        //            ReadResponseAsByte(resp, cancelToken.Token, stream);
        //            return Encoding.UTF8.GetString(stream.ToArray());
        //        }
        //        catch
        //        {
        //            //// Cancellation token.
        //            return "7035ba55-7d63-4349-9f73-c454529d4b2e";
        //        }
        //    }
        //}

        //private static void ReadResponseAsByte(WebResponse resp, CancellationToken token, Stream outputStream = null)
        //{
        //    using (Stream responseStream = resp.GetResponseStream())
        //    {
        //        var buffer = new byte[65536];
        //        int bytesRead;

        //        while (responseStream != null && (bytesRead = responseStream.Read(buffer, 0, buffer.Length)) > 0)
        //        {
        //            token.ThrowIfCancellationRequested();
        //            outputStream?.Write(buffer, 0, bytesRead);
        //        }
        //    }
        //}


        //// ReSharper disable once UnusedMethodReturnValue.Local
        //private long WriteBytesInStream(byte[] bytes, Stream outputStream, CancellationToken token, long length)
        //{
        //    BufferSize -= bytes.Length;
        //    Stream stream = null;

        //    try
        //    {
        //        stream = new MemoryStream(bytes);
        //        using (var source = new BinaryReader(stream))
        //        {
        //            stream = null;
        //            return WriteBytesInStream(source, outputStream, token, length);
        //        }
        //    }
        //    finally
        //    {
        //        stream?.Dispose();
        //    }
        //}

        //private long WriteBytesInStream(BinaryReader sourceStream, Stream outputStream, CancellationToken token, long length)
        //{
        //    int bufferLength = 65536;
        //    var totalWritten = 0L;
        //    if (length < bufferLength)
        //    {
        //        var z = sourceStream.ReadBytes((int)length);
        //        outputStream.Write(z, 0, (int)length);
        //    }
        //    else
        //    {
        //        while (length > totalWritten)
        //        {
        //            token.ThrowIfCancellationRequested();

        //            var bytes = sourceStream.ReadBytes(bufferLength);
        //            outputStream.Write(bytes, 0, bufferLength);

        //            totalWritten += bufferLength;
        //            if (length - totalWritten < bufferLength)
        //            {
        //                bufferLength = (int)(length - totalWritten);
        //            }
        //        }


        //    }
        //    return totalWritten;
        //}


        public override void Flush()
        {
            throw new NotImplementedException();
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotImplementedException();
        }

        public override void SetLength(long value)
        {
            _file.OriginalSize = value;
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            throw new NotImplementedException();
        }

        public override bool CanRead => true;
        public override bool CanSeek => true;
        public override bool CanWrite => true;
        public override long Length => _file.OriginalSize;
        public override long Position { get; set; }

        public static long BytesCount(string value)
        {
            return Encoding.UTF8.GetByteCount(value);
        }
    }
}
