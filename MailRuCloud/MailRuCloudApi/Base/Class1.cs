// Decompiled with JetBrains decompiler
// Type: System.Net.Http.PushStreamContent
// Assembly: System.Net.Http.Formatting, Version=5.2.4.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: FC82F88E-6F07-4942-9E94-C00AFE6E1DD7
// Assembly location: C:\Users\yar229\.nuget\packages\microsoft.aspnet.webapi.client\5.2.4-alpha1-170218\lib\netstandard1.1\System.Net.Http.Formatting.dll

using System.IO;
using System.Net.Http.Headers;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace System.Net.Http
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
          : this(PushStreamContent.Taskify(onStreamAvailable), (MediaTypeHeaderValue)null)
        {
        }

        public PushStreamContent(Func<Stream, HttpContent, TransportContext, Task> onStreamAvailable)
          : this(onStreamAvailable, (MediaTypeHeaderValue)null)
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
          : this(PushStreamContent.Taskify(onStreamAvailable), mediaType)
        {
        }

        public PushStreamContent(Func<Stream, HttpContent, TransportContext, Task> onStreamAvailable, MediaTypeHeaderValue mediaType)
        {
            if (onStreamAvailable == null)
                throw new ArgumentNullException(nameof(onStreamAvailable));
            this._onStreamAvailable = onStreamAvailable;
            this.Headers.ContentType = mediaType ?? new MediaTypeHeaderValue("application/octet-stream");
        }

        private static Func<Stream, HttpContent, TransportContext, Task> Taskify(Action<Stream, HttpContent, TransportContext> onStreamAvailable)
        {
            if (onStreamAvailable == null)
                throw new ArgumentNullException(nameof(onStreamAvailable));
            return (Func<Stream, HttpContent, TransportContext, Task>)((stream, content, transportContext) =>
            {
                onStreamAvailable(stream, content, transportContext);
                return Completed();
            });
        }

        internal static Task Completed()
        {
            return _defaultCompleted;
        }

        private static readonly Task _defaultCompleted = (Task)Task.FromResult<AsyncVoid>(new AsyncVoid());
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
            await this._onStreamAvailable((Stream)new PushStreamContent.CompleteTaskOnCloseStream(stream, serializeToStreamTask), (HttpContent)this, context);
            int num = await serializeToStreamTask.Task ? 1 : 0;
        }

        /// <summary>Computes the length of the stream if possible.</summary>
        /// <param name="length">The computed length of the stream.</param>
        /// <returns><c>true</c> if the length has been computed; otherwise <c>false</c>.</returns>
        protected override bool TryComputeLength(out long length)
        {
            length = -1L;
            return false;
        }

        internal class CompleteTaskOnCloseStream : DelegatingStream
        {
            private TaskCompletionSource<bool> _serializeToStreamTask;

            public CompleteTaskOnCloseStream(Stream innerStream, TaskCompletionSource<bool> serializeToStreamTask)
              : base(innerStream)
            {
                this._serializeToStreamTask = serializeToStreamTask;
            }

            protected override void Dispose(bool disposing)
            {
                this._serializeToStreamTask.TrySetResult(true);
            }
        }
    }

    internal abstract class DelegatingStream : Stream
    {
        private Stream _innerStream;

        protected DelegatingStream(Stream innerStream)
        {
            if (innerStream == null)
                throw new ArgumentNullException(nameof(innerStream));
            this._innerStream = innerStream;
        }

        protected Stream InnerStream
        {
            get
            {
                return this._innerStream;
            }
        }

        public override bool CanRead
        {
            get
            {
                return this._innerStream.CanRead;
            }
        }

        public override bool CanSeek
        {
            get
            {
                return this._innerStream.CanSeek;
            }
        }

        public override bool CanWrite
        {
            get
            {
                return this._innerStream.CanWrite;
            }
        }

        public override long Length
        {
            get
            {
                return this._innerStream.Length;
            }
        }

        public override long Position
        {
            get
            {
                return this._innerStream.Position;
            }
            set
            {
                this._innerStream.Position = value;
            }
        }

        public override int ReadTimeout
        {
            get
            {
                return this._innerStream.ReadTimeout;
            }
            set
            {
                this._innerStream.ReadTimeout = value;
            }
        }

        public override bool CanTimeout
        {
            get
            {
                return this._innerStream.CanTimeout;
            }
        }

        public override int WriteTimeout
        {
            get
            {
                return this._innerStream.WriteTimeout;
            }
            set
            {
                this._innerStream.WriteTimeout = value;
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                this._innerStream.Dispose();
            base.Dispose(disposing);
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            return this._innerStream.Seek(offset, origin);
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            return this._innerStream.Read(buffer, offset, count);
        }

        public override Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            return this._innerStream.ReadAsync(buffer, offset, count, cancellationToken);
        }

        public override int ReadByte()
        {
            return this._innerStream.ReadByte();
        }

        public override void Flush()
        {
            this._innerStream.Flush();
        }

        public override Task CopyToAsync(Stream destination, int bufferSize, CancellationToken cancellationToken)
        {
            return this._innerStream.CopyToAsync(destination, bufferSize, cancellationToken);
        }

        public override Task FlushAsync(CancellationToken cancellationToken)
        {
            return this._innerStream.FlushAsync(cancellationToken);
        }

        public override void SetLength(long value)
        {
            this._innerStream.SetLength(value);
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            this._innerStream.Write(buffer, offset, count);
        }

        public override Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            return this._innerStream.WriteAsync(buffer, offset, count, cancellationToken);
        }

        public override void WriteByte(byte value)
        {
            this._innerStream.WriteByte(value);
        }
    }
}
