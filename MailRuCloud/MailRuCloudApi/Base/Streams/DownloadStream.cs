using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using YaR.MailRuCloud.Api.Common;

namespace YaR.MailRuCloud.Api.Base.Streams
{
    internal class DownloadStream : Stream
    {
        //private static readonly log4net.ILog Logger = log4net.LogManager.GetLogger(typeof(DownloadStream));

        private const int InnerBufferSize = 65536 * 2;

        private readonly Func<long, long, File, CustomDisposable<HttpWebResponse>> _responseGenerator;
        private readonly IList<File> _files;
        private readonly long? _start;
        private readonly long? _end;

        private RingBufferedStream _innerStream;
        private bool _initialized;

        public DownloadStream(Func<long, long, File, CustomDisposable<HttpWebResponse>> responseGenerator, File file, long? start = null, long? end = null)
            : this(responseGenerator, file.Parts, start, end)
        {
        }

        private DownloadStream(Func<long, long, File, CustomDisposable<HttpWebResponse>> responseGenerator, IList<File> files, long? start = null, long? end = null)
        {
            var globalLength = files.Sum(f => f.OriginalSize);

            _responseGenerator = responseGenerator ?? throw new ArgumentNullException(nameof(responseGenerator));
            _files = files;
            _start = start;
            _end = end >= globalLength ? globalLength - 1 : end;

            Length = _start != null && _end != null
                ? _end.Value - _start.Value + 1
                : globalLength;

            Open();
        }

        public void Open()
        {
            _innerStream = new RingBufferedStream(InnerBufferSize) {ReadTimeout = 15 * 1000, WriteTimeout = 15 * 1000};
            _copyTask = GetFileStream();

            _initialized = true;
        }

        private Task _copyTask;

        private async Task<object> GetFileStream()
        {
            var totalLength = Length;
            long glostart = _start ?? 0;
            long gloend = _end == null || 
                _start == _end && _end == 0 ? totalLength : _end.Value + 1;

            long fileStart = 0;
            long fileEnd = 0;

            foreach (var file in _files)
            {
                var clofile = file;

                fileEnd += clofile.OriginalSize;

                if (glostart >= fileEnd || gloend <= fileStart)
                {
                    fileStart += clofile.OriginalSize;
                    continue;
                }
                
                long clostart = Math.Max(0, glostart - fileStart);
                long cloend = gloend - fileStart - 1;

                await Download(clostart, cloend, clofile).ConfigureAwait(false);

                fileStart += file.OriginalSize;
            }

            _innerStream.Flush();

            return _innerStream;
        }

        private async Task Download(long start, long end, File file)
        {
            using (var httpweb = _responseGenerator(start, end, file))
            using (var responseStream = httpweb.Value.GetResponseStream())
            {
                await responseStream.CopyToAsync(_innerStream).ConfigureAwait(false);
            }
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (!disposing) return;

            _innerStream?.Flush();
            _copyTask?.Wait();

            _innerStream?.Close();
        }


        public override void Flush()
        {
            throw new NotImplementedException();
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            return _innerStream.Seek(offset, origin);
        }

        public override void SetLength(long value)
        {
            throw new NotImplementedException();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            if (!_initialized)
                Open();

            int readed = _innerStream.Read(buffer, offset, count);
            return readed;
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotImplementedException();
        }

        public override bool CanRead { get; } = true;
        public override bool CanSeek { get; } = true;
        public override bool CanWrite { get; } = false;

        public override long Length { get; }

        public override long Position { get; set; }
    }
}
