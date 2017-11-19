using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace YaR.MailRuCloud.Api.Base.Threads
{
    /// <summary>
    /// Provides an <see cref="T:System.Net.Http.HttpContent" /> implementation that exposes an output <see cref="T:System.IO.Stream" />
    /// which can be written to directly. The ability to push data to the output stream differs from the
    /// <see cref="T:System.Net.Http.StreamContent" /> where data is pulled and not pushed.
    /// </summary>
    public class PushStreamContent : HttpContent
    {
        private readonly Func<Stream, HttpContent, TransportContext, Task> _onStreamAvailable;

        public PushStreamContent(Action<Stream, HttpContent, TransportContext> onStreamAvailable)
          : this(Taskify(onStreamAvailable))
        {
        }

        public PushStreamContent(Action<Stream, HttpContent, TransportContext> onStreamAvailable, string mediaType)
          : this(PushStreamContent.Taskify(onStreamAvailable), new MediaTypeHeaderValue(mediaType))
        {
        }

        public PushStreamContent(Func<Stream, HttpContent, TransportContext, Task> onStreamAvailable, string mediaType)
          : this(onStreamAvailable, new MediaTypeHeaderValue(mediaType))
        {
        }

        public PushStreamContent(Action<Stream, HttpContent, TransportContext> onStreamAvailable, MediaTypeHeaderValue mediaType)
          : this(Taskify(onStreamAvailable), mediaType)
        {
        }

        public PushStreamContent(Func<Stream, HttpContent, TransportContext, Task> onStreamAvailable, MediaTypeHeaderValue mediaType = (MediaTypeHeaderValue)null)
        {
            _onStreamAvailable = onStreamAvailable ?? throw new ArgumentNullException(nameof(onStreamAvailable));
            Headers.ContentType = mediaType ?? new MediaTypeHeaderValue("application/octet-stream");
        }

        private static Func<Stream, HttpContent, TransportContext, Task> Taskify(Action<Stream, HttpContent, TransportContext> onStreamAvailable)
        {
            if (onStreamAvailable == null)
                throw new ArgumentNullException(nameof(onStreamAvailable));
            return (stream, content, transportContext) =>
            {
                onStreamAvailable(stream, content, transportContext);
                return Completed();
            };
        }

        private static Task Completed()
        {
            return DefaultCompleted;
        }

        private static readonly Task DefaultCompleted = Task.FromResult(new AsyncVoid());
        [StructLayout(LayoutKind.Sequential, Size = 1)]
        private struct AsyncVoid
        {
        }

        /// <summary>
        /// When this method is called, it calls the action provided in the constructor with the output
        /// stream to write to. Once the action has completed its work it closes the stream which will
        /// close this content instance and complete the HTTP request or response.
        /// </summary>
        /// <param name="stream">The <see cref="T:System.IO.Stream" /> to which to write.</param>
        /// <param name="context">The associated <see cref="T:System.Net.TransportContext" />.</param>
        /// <returns>A <see cref="T:System.Threading.Tasks.Task" /> instance that is asynchronously serializing the object's content.</returns>
        protected override async Task SerializeToStreamAsync(Stream stream, TransportContext context)
        {
            TaskCompletionSource<bool> serializeToStreamTask = new TaskCompletionSource<bool>();
            await _onStreamAvailable(new CompleteTaskOnCloseStream(stream, serializeToStreamTask), this, context);
            await serializeToStreamTask.Task;
        }

        /// <summary>Computes the length of the stream if possible.</summary>
        /// <param name="length">The computed length of the stream.</param>
        /// <returns><c>true</c> if the length has been computed; otherwise <c>false</c>.</returns>
        protected override bool TryComputeLength(out long length)
        {
            length = -1L;
            return false;
        }

        private class CompleteTaskOnCloseStream : DelegatingStream
        {
            private readonly TaskCompletionSource<bool> _serializeToStreamTask;

            public CompleteTaskOnCloseStream(Stream innerStream, TaskCompletionSource<bool> serializeToStreamTask)
              : base(innerStream)
            {
                _serializeToStreamTask = serializeToStreamTask;
            }

            protected override void Dispose(bool disposing)
            {
                _serializeToStreamTask.TrySetResult(true);
            }
        }
    }

    internal abstract class DelegatingStream : Stream
    {
        protected DelegatingStream(Stream innerStream)
        {
            InnerStream = innerStream ?? throw new ArgumentNullException(nameof(innerStream));
        }

        private Stream InnerStream { get; }

        public override bool CanRead => InnerStream.CanRead;

        public override bool CanSeek => InnerStream.CanSeek;

        public override bool CanWrite => InnerStream.CanWrite;

        public override long Length => InnerStream.Length;

        public override long Position
        {
            get => InnerStream.Position;
            set => InnerStream.Position = value;
        }

        public override int ReadTimeout
        {
            get => InnerStream.ReadTimeout;
            set => InnerStream.ReadTimeout = value;
        }

        public override bool CanTimeout => InnerStream.CanTimeout;

        public override int WriteTimeout
        {
            get => InnerStream.WriteTimeout;
            set => InnerStream.WriteTimeout = value;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                InnerStream.Dispose();
            base.Dispose(disposing);
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            return InnerStream.Seek(offset, origin);
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            return InnerStream.Read(buffer, offset, count);
        }

        public override Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            return InnerStream.ReadAsync(buffer, offset, count, cancellationToken);
        }

        public override int ReadByte()
        {
            return InnerStream.ReadByte();
        }

        public override void Flush()
        {
            InnerStream.Flush();
        }

        public override Task CopyToAsync(Stream destination, int bufferSize, CancellationToken cancellationToken)
        {
            return InnerStream.CopyToAsync(destination, bufferSize, cancellationToken);
        }

        public override Task FlushAsync(CancellationToken cancellationToken)
        {
            return InnerStream.FlushAsync(cancellationToken);
        }

        public override void SetLength(long value)
        {
            InnerStream.SetLength(value);
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            InnerStream.Write(buffer, offset, count);
        }

        public override Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            return InnerStream.WriteAsync(buffer, offset, count, cancellationToken);
        }

        public override void WriteByte(byte value)
        {
            InnerStream.WriteByte(value);
        }
    }
}
