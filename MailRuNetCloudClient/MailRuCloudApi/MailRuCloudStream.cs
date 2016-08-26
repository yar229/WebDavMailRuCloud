using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace MailRuCloudApi
{
    internal class MailRuCloudStream : Stream
    {
        private readonly string _fileName;
        private readonly string _destinationPath;
        private readonly string _extension;
        private readonly ShardInfo _shard;
        private readonly Account _account;
        private long _size;
        private readonly CancellationTokenSource _cancelToken;

        public MailRuCloudStream(string fileName, string destinationPath, string extension, ShardInfo shard, Account account, CancellationTokenSource cancelToken, long size)
        {
            _fileName = fileName;
            _destinationPath = destinationPath;
            _extension = extension;
            _shard = shard;
            _account = account;
            _cancelToken = cancelToken;
            _size = size;
            Initialize();
        }

        private HttpWebRequest _request;
        private byte[] _endBoundaryRequest;


        
        private void Initialize()
        {
            //_destinationPath = _destinationPath.EndsWith("/") ? _destinationPath : _destinationPath + "/";
            var maxFileSize = 2L * 1024L * 1024L * 1024L;
            if (_size > maxFileSize)
            {
                throw new OverflowException("Not supported file size.", new Exception($"The maximum file size is {maxFileSize} byte. Currently file size is {_size} byte."));
            }

            var boundary = Guid.NewGuid();

            //// Boundary request building.
            var boundaryBuilder = new StringBuilder();
            boundaryBuilder.AppendFormat("------{0}\r\n", boundary);
            boundaryBuilder.AppendFormat("Content-Disposition: form-data; name=\"file\"; filename=\"{0}\"\r\n",  "/" + _fileName);
            boundaryBuilder.AppendFormat("Content-Type: {0}\r\n\r\n", ConstSettings.GetContentType(_extension));

            var endBoundaryBuilder = new StringBuilder();
            endBoundaryBuilder.AppendFormat("\r\n------{0}--\r\n", boundary);

            _endBoundaryRequest = Encoding.UTF8.GetBytes(endBoundaryBuilder.ToString());
            var boundaryRequest = Encoding.UTF8.GetBytes(boundaryBuilder.ToString());

            var url = new Uri($"{_shard.Url}?cloud_domain=2&{_account.LoginName}");
            _request = (HttpWebRequest)WebRequest.Create(url.OriginalString);
            _request.Proxy = _account.Proxy;
            _request.CookieContainer = _account.Cookies;
            _request.Method = "POST";

            _request.ContentLength = _size + boundaryRequest.LongLength + _endBoundaryRequest.LongLength;
            //_request.SendChunked = true;

            _request.Referer = $"{ConstSettings.CloudDomain}/home/{HttpUtility.UrlEncode(_destinationPath)}";
            _request.Headers.Add("Origin", ConstSettings.CloudDomain);
            _request.Host = url.Host;
            _request.ContentType = $"multipart/form-data; boundary=----{boundary}";
            _request.Accept = "*/*";
            _request.UserAgent = ConstSettings.UserAgent;
            _request.AllowWriteStreamBuffering = false;


            //_request.GetRequestStream();

            _task = Task.Factory.FromAsync(
                _request.BeginGetRequestStream, 
                asyncResult => 
                _request.EndGetRequestStream(asyncResult), 
                null);

            _task =  _task.ContinueWith
                (
                            (t, m) =>
                            {
                                try
                                {
                                    var token = (CancellationToken)m;
                                    var s = t.Result;
                                    WriteBytesInStream(boundaryRequest, s, token, boundaryRequest.Length);
                                }
                                catch (Exception ex)
                                {
                                    return (Stream)null;
                                }
                                //finally
                                //{
                                //    //t.Wait();
                                //    //var x = t.Result;
                                //    //return t.Result;
                                //    //return x;
                                //    //x.Dispose();
                                //}

                                //return true;
                                return t.Result;
                            },
                        _cancelToken.Token, TaskContinuationOptions.OnlyOnRanToCompletion);
        }

        private Task<Stream> _task;
        //private Task<bool> _taskc;



        public override void Write(byte[] buffer, int offset, int count)
        {

            var zbuffer = new byte[buffer.Length];
            buffer.CopyTo(zbuffer, 0);
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
                                catch (Exception ex)
                                {
                                    return (Stream)null;
                                }
                                //finally
                                //{
                                //    var x = t.Result;
                                //    //x.Dispose();
                                //}

                                return t.Result;
                            },
                        _cancelToken.Token, TaskContinuationOptions.OnlyOnRanToCompletion);
        }
        public override void Close()
        {


           var z =   _task.ContinueWith(
                (t, m) =>
                {
                    try
                    {
                        var token = (CancellationToken) m;
                        var s = t.Result;

                        WriteBytesInStream(_endBoundaryRequest, s, token, _endBoundaryRequest.Length);
                        

                        using (var response = (HttpWebResponse)_request.GetResponse())
                        {
                            if (response.StatusCode == HttpStatusCode.OK)
                            {
                                var resp = ReadResponseAsText(response, _cancelToken).Split(';');
                                var hashResult = resp[0];
                                var sizeResult = long.Parse(resp[1].Replace("\r\n", string.Empty));

                                return this.AddFileInCloud(new File
                                {
                                    Name = _fileName,
                                    FulPath = HttpUtility.UrlDecode(_destinationPath),
                                    Hash = hashResult,
                                    Size = new FileSize
                                    {
                                        DefaultValue = sizeResult
                                    }
                                }).Result;
                            }
                        }

                    }
                    catch (Exception ex)
                    {
                        return false;
                    }
                    finally
                    {
                        var st = t.Result;
                        st?.Close();
                        st?.Dispose();
                    }

                    return true;
                },
            _cancelToken.Token, TaskContinuationOptions.OnlyOnRanToCompletion);

            z.Wait();

            base.Close();
        }

        public enum ResolveFileConflictMethod 
        {
            Rename,
            Rewrite
        }
        private async Task<bool> AddFileInCloud(File fileInfo, ResolveFileConflictMethod conflict = ResolveFileConflictMethod.Rewrite)
        {
            fileInfo.FulPath = fileInfo.FulPath.Replace("\\", "/");


            var hasFile = fileInfo.Hash != null && fileInfo.Size.DefaultValue != 0;
            var filePart = hasFile ? $"&hash={fileInfo.Hash}&size={fileInfo.Size.DefaultValue}" : string.Empty;

            //var addFileRequest = Encoding.UTF8.GetBytes($"home={fileInfo.FulPath}&conflict=rename&api={2}&token={_account.AuthToken}" + filePart);
            string conflictstr;
            switch (conflict)
            {
                case ResolveFileConflictMethod.Rewrite:
                    conflictstr = "rewrite"; break;
                case ResolveFileConflictMethod.Rename:
                    conflictstr = "rename"; break;
                default: throw new NotImplementedException("File conflict method not implemented");
            }

            var addFileRequest = Encoding.UTF8.GetBytes($"home={fileInfo.FulPath}&conflict=rewrite&api={2}&token={_account.AuthToken}" + filePart);

            var url = new Uri($"{ConstSettings.CloudDomain}/api/v2/{(hasFile ? "file" : "folder")}/add");
            var request = (HttpWebRequest)WebRequest.Create(url.OriginalString);
            request.Proxy = _account.Proxy;
            request.CookieContainer = _account.Cookies;
            request.Method = "POST";
            request.ContentLength = addFileRequest.LongLength;
            request.Referer = string.Format("{0}/home{1}", ConstSettings.CloudDomain, HttpUtility.UrlEncode(fileInfo.FulPath.Substring(0, fileInfo.FulPath.LastIndexOf(fileInfo.Name))));
            request.Headers.Add("Origin", ConstSettings.CloudDomain);
            request.Host = url.Host;
            request.ContentType = ConstSettings.DefaultRequestType;
            request.Accept = "*/*";
            request.UserAgent = ConstSettings.UserAgent;
            var task = Task.Factory.FromAsync(request.BeginGetRequestStream, asyncResult => request.EndGetRequestStream(asyncResult), (object)null);
            return await task.ContinueWith((t) =>
            {
                using (var s = t.Result)
                {
                    s.Write(addFileRequest, 0, addFileRequest.Length);
                    using (var response = (HttpWebResponse)request.GetResponse())
                    {
                        if (response.StatusCode != HttpStatusCode.OK)
                        {
                            throw new Exception();
                        }

                        return true;
                    }
                }
            });
        }


        internal string ReadResponseAsText(WebResponse resp, CancellationTokenSource cancelToken)
        {
            using (var stream = new MemoryStream())
            {
                try
                {
                    this.ReadResponseAsByte(resp, cancelToken.Token, stream);
                    return Encoding.UTF8.GetString(stream.ToArray());
                }
                catch
                {
                    //// Cancellation token.
                    return "7035ba55-7d63-4349-9f73-c454529d4b2e";
                }
            }
        }

        internal void ReadResponseAsByte(WebResponse resp, CancellationToken token, Stream outputStream = null, long contentLength = 0, OperationType operation = OperationType.None)
        {
            if (contentLength != 0 && outputStream.Position == 0)
            {
                //this.OnChangedProgressPercent(new ProgressChangedEventArgs(
                //                0,
                //                new ProgressChangeTaskState()
                //                {
                //                    Type = operation,
                //                    TotalBytes = new FileSize()
                //                    {
                //                        DefaultValue = contentLength
                //                    },
                //                    BytesInProgress = new FileSize()
                //                    {
                //                        DefaultValue = 0L
                //                    }
                //                }));
            }

            int bufSizeChunk = 30000;
            int totalBufSize = bufSizeChunk;
            byte[] fileBytes = new byte[totalBufSize];
            double percentComplete = 0;

            int totalBytesRead = 0;

            using (var reader = new BinaryReader(resp.GetResponseStream()))
            {
                int bytesRead = 0;
                while ((bytesRead = reader.Read(fileBytes, totalBytesRead, totalBufSize - totalBytesRead)) > 0)
                {
                    token.ThrowIfCancellationRequested();

                    if (outputStream != null)
                    {
                        outputStream.Write(fileBytes, totalBytesRead, bytesRead);
                    }

                    totalBytesRead += bytesRead;

                    if ((totalBufSize - totalBytesRead) == 0)
                    {
                        totalBufSize += bufSizeChunk;
                        Array.Resize(ref fileBytes, totalBufSize);
                    }

                    if (contentLength != 0 && contentLength >= outputStream.Position)
                    {
                        var tempPercentComplete = 100.0 * (double)outputStream.Position / (double)contentLength;
                        if (tempPercentComplete - percentComplete >= 1)
                        {
                            //percentComplete = tempPercentComplete;
                            //this.OnChangedProgressPercent(new ProgressChangedEventArgs(
                            //    (int)percentComplete,
                            //    new ProgressChangeTaskState()
                            //    {
                            //        Type = operation,
                            //        TotalBytes = new FileSize()
                            //        {
                            //            DefaultValue = contentLength
                            //        },
                            //        BytesInProgress = new FileSize()
                            //        {
                            //            DefaultValue = outputStream.Position
                            //        }
                            //    }));
                        }
                    }
                }

                if (contentLength != 0 && outputStream.Position == contentLength)
                {
                    //this.OnChangedProgressPercent(new ProgressChangedEventArgs(
                    //            100,
                    //            new ProgressChangeTaskState()
                    //            {
                    //                Type = operation,
                    //                TotalBytes = new FileSize()
                    //                {
                    //                    DefaultValue = contentLength
                    //                },
                    //                BytesInProgress = new FileSize()
                    //                {
                    //                    DefaultValue = outputStream.Position
                    //                }
                    //            }));
                }
            }
        }

        private long WriteBytesInStream(byte[] bytes, Stream outputStream, CancellationToken token, long length, bool includeProgressEvent = false, OperationType operation = OperationType.None)
        {
            using (var stream = new MemoryStream(bytes))
            {
                using (var source = new BinaryReader(stream))
                {
                    return this.WriteBytesInStream(source, outputStream, token, length, includeProgressEvent, operation);
                }
            }
        }

        private long WriteBytesInStream(BinaryReader sourceStream, Stream outputStream, CancellationToken token, long length, bool includeProgressEvent = false, OperationType operation = OperationType.None)
        {
            if (includeProgressEvent && (sourceStream.BaseStream.Length == length || sourceStream.BaseStream.Position == 0))
            {
                //this.OnChangedProgressPercent(new ProgressChangedEventArgs(
                //                0,
                //                new ProgressChangeTaskState()
                //                {
                //                    Type = operation,
                //                    TotalBytes = new FileSize()
                //                    {
                //                        DefaultValue = sourceStream.BaseStream.Length
                //                    },
                //                    BytesInProgress = new FileSize()
                //                    {
                //                        DefaultValue = 0L
                //                    }
                //                }));
            }

            int bufferLength = 8192;
            var totalWritten = 0L;
            if (length < bufferLength)
            {
                //sourceStream.BaseStream.CopyTo(outputStream);
                var z = sourceStream.ReadBytes((int) length);
                outputStream.Write(z, 0, (int)length);
            }
            else
            {
                double percentComplete = 0;
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

                    if (includeProgressEvent && length != 0 && sourceStream.BaseStream.Length >= sourceStream.BaseStream.Position)
                    {
                        double tempPercentComplete = 100.0 * (double)sourceStream.BaseStream.Position / (double)sourceStream.BaseStream.Length;
                        if (tempPercentComplete - percentComplete >= 1)
                        {
                            percentComplete = tempPercentComplete;
                            //this.OnChangedProgressPercent(new ProgressChangedEventArgs(
                            //    (int)percentComplete,
                            //    new ProgressChangeTaskState()
                            //    {
                            //        Type = operation,
                            //        TotalBytes = new FileSize()
                            //        {
                            //            DefaultValue = sourceStream.BaseStream.Length
                            //        },
                            //        BytesInProgress = new FileSize()
                            //        {
                            //            DefaultValue = sourceStream.BaseStream.Position
                            //        }
                            //    }));
                        }
                    }
                }

                
            }

            if (includeProgressEvent && (sourceStream.BaseStream.Length == length || sourceStream.BaseStream.Position == sourceStream.BaseStream.Length))
            {
                //this.OnChangedProgressPercent(new ProgressChangedEventArgs(
                //                100,
                //                new ProgressChangeTaskState()
                //                {
                //                    Type = operation,
                //                    TotalBytes = new FileSize()
                //                    {
                //                        DefaultValue = sourceStream.BaseStream.Length
                //                    },
                //                    BytesInProgress = new FileSize()
                //                    {
                //                        DefaultValue = sourceStream.BaseStream.Position == 0 ? length : sourceStream.BaseStream.Position
                //                    }
                //                }));
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
            _size = value;
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            throw new NotImplementedException();
        }







        public override bool CanRead => true;
        public override bool CanSeek => true;
        public override bool CanWrite => true;
        public override long Length => _size;
        public override long Position { get; set; }


        protected override void Dispose(bool disposing)
        {

           

            base.Dispose(disposing);
        }


    }
}
