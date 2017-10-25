using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MailRuCloudApi.Api.Requests;

namespace MailRuCloudApi.Api
{
    internal class UploadStream : Stream
    {
        private readonly CloudApi _cloud;
        private readonly File _file;
        private readonly ShardInfo _shard;

        public UploadStream(string destinationPath, CloudApi cloud, long size)
        {
            _cloud = cloud;

            _file = new File(destinationPath, size, null);
            _shard = _cloud.GetShardInfo(ShardType.Upload).Result;

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

            _task = Task.Factory.FromAsync(_request.BeginGetRequestStream, asyncResult => _request.EndGetRequestStream(asyncResult), null);

            _task = _task.ContinueWith
                (
                            (t, m) =>
                            {
                                try
                                {
                                    var token = (CancellationToken)m;
                                    var s = t.Result;
                                    WriteBytesInStream(boundaryRequest, s, token, boundaryRequest.Length);
                                }
                                catch (Exception)
                                {
                                    return (Stream)null;
                                }
                                return t.Result;
                            },
                        _cloud.CancelToken.Token, TaskContinuationOptions.OnlyOnRanToCompletion);
        }

        private Task<Stream> _task;

        private const long MaxBufferSize = 3000000;

        private readonly AutoResetEvent _canWrite = new AutoResetEvent(true);

        private long BufferSize
        {
            set
            {
                lock (_bufferSizeLocker)
                {
                    _bufferSize = value;
                    if (_bufferSize > MaxBufferSize)
                        _canWrite.Reset();
                    else _canWrite.Set();
                }
            }
            get
            {
                lock (_bufferSizeLocker)
                {
                    return _bufferSize;
                }
            }
        }

        private long _bufferSize;

        private readonly object _bufferSizeLocker = new object();

        public override void Write(byte[] buffer, int offset, int count)
        {
            _canWrite.WaitOne();
            BufferSize += count;

            var zbuffer = new byte[count];
            Array.Copy(buffer, offset, zbuffer, 0, count); //buffer.CopyTo(zbuffer, 0);
            var zcount = count;
            _task = _task.ContinueWith(
                            (t, m) =>
                            {
                                try
                                {
                                    var token = (CancellationToken)m;
                                    var s = t.Result;
                                    WriteBytesInStream(zbuffer, s, token, zcount);
                                }
                                // ReSharper disable once UnusedVariable
                                catch (Exception ex)
                                {
                                    return null;
                                }

                                return t.Result;
                            },
                        _cloud.CancelToken.Token, TaskContinuationOptions.OnlyOnRanToCompletion);
        }
        public override void Close()
        {
            var z = _task.ContinueWith(
                 (t, m) =>
                 {
                     try
                     {
                         var token = (CancellationToken)m;
                         var s = t.Result;
                         WriteBytesInStream(_endBoundaryRequest, s, token, _endBoundaryRequest.Length);
                     }
                     catch (Exception)
                     {
                         return false;
                     }
                     finally
                     {
                         var st = t.Result;
                         st?.Dispose();
                     }


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
                             return res;
                         }
                     }

                     return true;
                 },
             _cloud.CancelToken.Token, TaskContinuationOptions.OnlyOnRanToCompletion);

            z.Wait();

            base.Close();
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


        // ReSharper disable once UnusedMethodReturnValue.Local
        private long WriteBytesInStream(byte[] bytes, Stream outputStream, CancellationToken token, long length)
        {
            BufferSize -= bytes.Length;
            Stream stream = null;

            try
            {
                stream = new MemoryStream(bytes);
                using (var source = new BinaryReader(stream))
                {
                    stream = null;
                    return WriteBytesInStream(source, outputStream, token, length);
                }
            }
            finally
            {
                stream?.Dispose();
            }
        }

        private long WriteBytesInStream(BinaryReader sourceStream, Stream outputStream, CancellationToken token, long length)
        {
            int bufferLength = 65536;
            var totalWritten = 0L;
            if (length < bufferLength)
            {
                var z = sourceStream.ReadBytes((int)length);
                outputStream.Write(z, 0, (int)length);
            }
            else
            {
                while (length > totalWritten)
                {
                    token.ThrowIfCancellationRequested();

                    var bytes = sourceStream.ReadBytes(bufferLength);
                    outputStream.Write(bytes, 0, bufferLength);

                    totalWritten += bufferLength;
                    if (length - totalWritten < bufferLength)
                    {
                        bufferLength = (int)(length - totalWritten);
                    }
                }


            }
            return totalWritten;
        }


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
            _file.Size = value;
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            throw new NotImplementedException();
        }

        public override bool CanRead => true;
        public override bool CanSeek => true;
        public override bool CanWrite => true;
        public override long Length => _file.Size;
        public override long Position { get; set; }

        public static long BytesCount(string value)
        {
            return Encoding.UTF8.GetByteCount(value);
        }
    }
}
