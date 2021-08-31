using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using YaR.Clouds.Extensions;

namespace YaR.Clouds.Base.Streams
{
    /// <summary>
    /// A ring-buffer stream that you can read from and write to from
    /// different threads.
    /// </summary>
    public class RingBufferedStream : Stream
    {
        private readonly byte[] _store;

        private readonly ManualResetEventAsync _writeAvailable = new(false);

        private readonly ManualResetEventAsync _readAvailable = new(false);

        private readonly ManualResetEvent _flushed = new(false);


        private readonly CancellationTokenSource _cancellationTokenSource = new();

        private int _readPos;

        private int _readAvailableByteCount;

        private int _writePos;

        private int _writeAvailableByteCount;

        private bool _disposed;

        /// <summary>
        /// Initializes a new instance of the <see cref="RingBufferedStream"/>
        /// class.
        /// </summary>
        /// <param name="bufferSize">
        /// The maximum number of bytes to buffer.
        /// </param>
        public RingBufferedStream(int bufferSize)
        {
            _store = new byte[bufferSize];
            _writeAvailableByteCount = bufferSize;
            _readAvailableByteCount = 0;
        }

        public override bool CanRead => true;

        public override bool CanSeek => false;

        public override bool CanWrite => true;

        public override long Length => throw new NotSupportedException("Cannot get length on RingBufferedStream");

        /// <inheritdoc/>
        public override int ReadTimeout { get; set; } = Timeout.Infinite;

        /// <inheritdoc/>
        public override int WriteTimeout { get; set; } = Timeout.Infinite;

        /// <inheritdoc/>
        public override long Position
        {
            get => throw new NotSupportedException("Cannot set position on RingBufferedStream");

            set => throw new NotSupportedException("Cannot set position on RingBufferedStream");
        }

        /// <inheritdoc/>
        public override void Flush()
        {
            if (!_disposed)
            {
                _flushed.Set();
                _cancellationTokenSource?.Cancel();
            }
        }

        /// <summary>
        /// Set the length of the current stream. Always throws <see
        /// cref="NotSupportedException"/>.
        /// </summary>
        /// <param name="value">
        /// The desired length of the current stream in bytes.
        /// </param>
        public override void SetLength(long value)
        {
            throw new NotSupportedException(
                "Cannot set length on RingBufferedStream");
        }

        /// <summary>
        /// Sets the position in the current stream. Always throws <see
        /// cref="NotSupportedException"/>.
        /// </summary>
        /// <param name="offset">
        /// The byte offset to the <paramref name="origin"/> parameter.
        /// </param>
        /// <param name="origin">
        /// A value of type <see cref="SeekOrigin"/> indicating the reference
        /// point used to obtain the new position.
        /// </param>
        /// <returns>
        /// The new position within the current stream.
        /// </returns>
        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotSupportedException("Cannot seek on RingBufferedStream");
        }

        /// <inheritdoc/>
        public override void Write(byte[] buffer, int offset, int count)
        {
            if (_disposed)
            {
                //throw new ObjectDisposedException("RingBufferedStream");
                return;
            }

            Monitor.Enter(_store);
            bool haveLock = true;
            try
            {
                while (count > 0)
                {
                    if (_writeAvailableByteCount == 0)
                    {
                        _writeAvailable.Reset();
                        Monitor.Exit(_store);
                        haveLock = false;
                        if (!_writeAvailable.Wait(WriteTimeout, _cancellationTokenSource.Token, out var canceled) || canceled)
                        {
                            break;
                        }

                        Monitor.Enter(_store);
                        haveLock = true;
                    }
                    else
                    {
                        var toWrite = _store.Length - _writePos;
                        if (toWrite > _writeAvailableByteCount)
                        {
                            toWrite = _writeAvailableByteCount;
                        }

                        if (toWrite > count)
                            toWrite = count;

                        Array.Copy(buffer, offset, _store, _writePos, toWrite);
                        offset += toWrite;
                        count -= toWrite;
                        _writeAvailableByteCount -= toWrite;
                        _readAvailableByteCount += toWrite;
                        _writePos += toWrite;
                        if (_writePos == _store.Length)
                            _writePos = 0;

                        _readAvailable.Set();
                    }
                }
            }
            finally
            {
                if (haveLock)
                {
                    Monitor.Exit(_store);
                }
            }
        }

        /// <inheritdoc/>
        public override void WriteByte(byte value)
        {
            if (_disposed)
                throw new ObjectDisposedException("RingBufferedStream");

            Monitor.Enter(_store);
            bool haveLock = true;
            try
            {
                while (true)
                {
                    if (_writeAvailableByteCount == 0)
                    {
                        _writeAvailable.Reset();
                        Monitor.Exit(_store);
                        haveLock = false;
                        if (!_writeAvailable.Wait(WriteTimeout, _cancellationTokenSource.Token, out var canceled) || canceled)
                            break;

                        Monitor.Enter(_store);
                        haveLock = true;
                    }
                    else
                    {
                        _store[_writePos] = value;
                        --_writeAvailableByteCount;
                        ++_readAvailableByteCount;
                        ++_writePos;
                        if (_writePos == _store.Length)
                            _writePos = 0;

                        _readAvailable.Set();
                        break;
                    }
                }
            }
            finally
            {
                if (haveLock)
                {
                    Monitor.Exit(_store);
                }
            }
        }

        /// <inheritdoc/>
        public override int Read(byte[] buffer, int offset, int count)
        {
            if (_disposed)
                throw new ObjectDisposedException("RingBufferedStream");

            Monitor.Enter(_store);
            int ret = 0;
            bool haveLock = true;
            try
            {
                while (count > 0 )
                {
                    if (_readAvailableByteCount == 0)
                    {
                        _readAvailable.Reset();
                        Monitor.Exit(_store);
                        haveLock = false;
                        if (!_readAvailable.Wait(ReadTimeout, _cancellationTokenSource.Token, out var canceled) || canceled)
                            break;

                        Monitor.Enter(_store);
                        haveLock = true;
                    }
                    else
                    {
                        var toRead = _store.Length - _readPos;
                        if (toRead > _readAvailableByteCount)
                            toRead = _readAvailableByteCount;

                        if (toRead > count)
                            toRead = count;

                        Array.Copy(_store, _readPos, buffer, offset, toRead);
                        offset += toRead;
                        count -= toRead;
                        _readAvailableByteCount -= toRead;
                        _writeAvailableByteCount += toRead;
                        ret += toRead;
                        _readPos += toRead;
                        if (_readPos == _store.Length)
                            _readPos = 0;

                        _writeAvailable.Set();

                        if (_flushed.WaitOne(0)) return ret;
                    }
                }
            }
            finally
            {
                if (haveLock)
                    Monitor.Exit(_store);
            }

            return ret;
        }

        /// <inheritdoc/>
        public override int ReadByte()
        {
            if (_disposed)
                throw new ObjectDisposedException("RingBufferedStream");

            Monitor.Enter(_store);
            int ret = -1;
            bool haveLock = true;
            try
            {
                while (true)
                {
                    if (_readAvailableByteCount == 0)
                    {
                        _readAvailable.Reset();
                        Monitor.Exit(_store);
                        haveLock = false;
                        if (!_readAvailable.Wait(ReadTimeout, _cancellationTokenSource.Token, out var canceled) || canceled)
                            break;

                        Monitor.Enter(_store);
                        haveLock = true;
                    }
                    else
                    {
                        ret = _store[_readPos];
                        ++_writeAvailableByteCount;
                        --_readAvailableByteCount;
                        ++_readPos;
                        if (_readPos == _store.Length)
                            _readPos = 0;

                        _writeAvailable.Set();
                        break;
                    }
                }
            }
            finally
            {
                if (haveLock)
                    Monitor.Exit(_store);
            }

            return ret;
        }

        //public override void Close()
        //{
        //    _cancellationTokenSource.Cancel();
        //    base.Close();
        //}

        /// <inheritdoc/>
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (!disposing) return;

            _disposed = true;
            //_cancellationTokenSource.Cancel();
            //_cancellationTokenSource.Dispose();
        }
    }

    public sealed class ManualResetEventAsync
    {
        /// <summary>
        /// The task completion source.
        /// </summary>
        private volatile TaskCompletionSource<bool> _taskCompletionSource =
            new();

        /// <summary>
        /// Initializes a new instance of the <see cref="ManualResetEventAsync"/>
        /// class with a <see cref="bool"/> value indicating whether to set the
        /// initial state to signaled.
        /// </summary>
        /// <param name="initialState">
        /// True to set the initial state to signaled; false to set the initial
        /// state to non-signaled.
        /// </param>
        public ManualResetEventAsync(bool initialState)
        {
            if (initialState)
            {
                Set();
            }
        }

        /// <summary>
        /// Return a task that can be consumed by <see cref="Task.Wait()"/>
        /// </summary>
        /// <returns>
        /// The asynchronous waiter.
        /// </returns>
        public Task GetWaitTask()
        {
            return _taskCompletionSource.Task;
        }

        /// <summary>
        /// Mark the event as signaled.
        /// </summary>
        public void Set()
        {
            var tcs = _taskCompletionSource;
            Task.Factory.StartNew(
                s => ((TaskCompletionSource<bool>)s).TrySetResult(true),
                tcs,
                CancellationToken.None,
                TaskCreationOptions.PreferFairness,
                TaskScheduler.Default);
            tcs.Task.Wait();
        }

        /// <summary>
        /// Mark the event as not signaled.
        /// </summary>
        public void Reset()
        {
            while (true)
            {
                var tcs = _taskCompletionSource;
                if (!tcs.Task.IsCompleted
#pragma warning disable 420
                || Interlocked.CompareExchange(
                        ref _taskCompletionSource,
                        new TaskCompletionSource<bool>(),
                        tcs) == tcs)
#pragma warning restore 420
                {
                    return;
                }
            }
        }

        /// <summary>
        /// Waits for the <see cref="ManualResetEventAsync"/> to be signaled.
        /// </summary>
        /// <exception cref="T:System.AggregateException">
        /// The <see cref="ManualResetEventAsync"/> waiting <see cref="Task"/>
        /// was canceled -or- an exception was thrown during the execution
        /// of the <see cref="ManualResetEventAsync"/> waiting <see cref="Task"/>.
        /// </exception>
        public void Wait()
        {
            GetWaitTask().Wait();
        }

        /// <summary>
        /// Waits for the <see cref="ManualResetEventAsync"/> to be signaled.
        /// </summary>
        /// <param name="cancellationToken">
        /// A <see cref="CancellationToken"/> to observe while waiting for
        /// the task to complete.
        /// </param>
        /// <exception cref="T:System.OperationCanceledException">
        /// The <paramref name="cancellationToken"/> was canceled.
        /// </exception>
        /// <exception cref="T:System.AggregateException">
        /// The <see cref="ManualResetEventAsync"/> waiting <see cref="Task"/> was
        /// canceled -or- an exception was thrown during the execution of the
        /// <see cref="ManualResetEventAsync"/> waiting <see cref="Task"/>.
        /// </exception>
        public void Wait(CancellationToken cancellationToken)
        {
            GetWaitTask().Wait(cancellationToken);
        }

        /// <summary>
        /// Waits for the <see cref="ManualResetEventAsync"/> to be signaled.
        /// </summary>
        /// <param name="cancellationToken">
        /// A <see cref="CancellationToken"/> to observe while waiting for
        /// the task to complete.
        /// </param>
        /// <param name="canceled">
        /// Set to true if the wait was canceled via the <paramref
        /// name="cancellationToken"/>.
        /// </param>
        public void Wait(CancellationToken cancellationToken, out bool canceled)
        {
            try
            {
                GetWaitTask().Wait(cancellationToken);
                canceled = false;
            }
            catch (Exception ex)
                when (ex is OperationCanceledException
                    || (ex is AggregateException
                        && ex.InnerOf<OperationCanceledException>() != null))
            {
                canceled = true;
            }
        }

        /// <summary>
        /// Waits for the <see cref="ManualResetEventAsync"/> to be signaled.
        /// </summary>
        /// <param name="timeout">
        /// A <see cref="System.TimeSpan"/> that represents the number of
        /// milliseconds to wait, or a <see cref="System.TimeSpan"/> that
        /// represents -1 milliseconds to wait indefinitely.
        /// </param>
        /// <returns>
        /// true if the <see cref="ManualResetEventAsync"/> was signaled within
        /// the allotted time; otherwise, false.
        /// </returns>
        /// <exception cref="T:System.ArgumentOutOfRangeException">
        /// <paramref name="timeout"/> is a negative number other than -1
        /// milliseconds, which represents an infinite time-out -or-
        /// timeout is greater than <see cref="int.MaxValue"/>.
        /// </exception>
        public bool Wait(TimeSpan timeout)
        {
            return GetWaitTask().Wait(timeout);
        }

        /// <summary>
        /// Waits for the <see cref="ManualResetEventAsync"/> to be signaled.
        /// </summary>
        /// <param name="millisecondsTimeout">
        /// The number of milliseconds to wait, or
        /// <see cref="System.Threading.Timeout.Infinite"/> (-1) to wait
        /// indefinitely.
        /// </param>
        /// <returns>
        /// true if the <see cref="ManualResetEventAsync"/> was signaled within
        /// the allotted time; otherwise, false.
        /// </returns>
        /// <exception cref="T:System.ArgumentOutOfRangeException">
        /// <paramref name="millisecondsTimeout"/> is a negative number other
        /// than -1, which represents an infinite time-out.
        /// </exception>
        public bool Wait(int millisecondsTimeout)
        {
            return GetWaitTask().Wait(millisecondsTimeout);
        }

        /// <summary>
        /// Waits for the <see cref="ManualResetEventAsync"/> to be signaled.
        /// </summary>
        /// <param name="millisecondsTimeout">
        /// The number of milliseconds to wait, or
        /// <see cref="System.Threading.Timeout.Infinite"/> (-1) to wait
        /// indefinitely.
        /// </param>
        /// <param name="cancellationToken">
        /// A <see cref="CancellationToken"/> to observe while waiting for the
        /// <see cref="ManualResetEventAsync"/> to be signaled.
        /// </param>
        /// <returns>
        /// true if the <see cref="ManualResetEventAsync"/> was signaled within
        /// the allotted time; otherwise, false.
        /// </returns>
        /// <exception cref="T:System.AggregateException">
        /// The <see cref="ManualResetEventAsync"/> waiting <see cref="Task"/>
        /// was canceled -or- an exception was thrown during the execution of
        /// the <see cref="ManualResetEventAsync"/> waiting <see cref="Task"/>.
        /// </exception>
        /// <exception cref="T:System.ArgumentOutOfRangeException">
        /// <paramref name="millisecondsTimeout"/> is a negative number other
        /// than -1, which represents an infinite time-out.
        /// </exception>
        /// <exception cref="T:System.OperationCanceledException">
        /// The <paramref name="cancellationToken"/> was canceled.
        /// </exception>
        public bool Wait(int millisecondsTimeout, CancellationToken cancellationToken)
        {
            return GetWaitTask().Wait(millisecondsTimeout, cancellationToken);
        }

        /// <summary>
        /// Waits for the <see cref="ManualResetEventAsync"/> to be signaled.
        /// </summary>
        /// <param name="millisecondsTimeout">
        /// The number of milliseconds to wait, or
        /// <see cref="System.Threading.Timeout.Infinite"/> (-1) to wait
        /// indefinitely.
        /// </param>
        /// <param name="cancellationToken">
        /// A <see cref="CancellationToken"/> to observe while waiting for the
        /// <see cref="ManualResetEventAsync"/> to be signaled.
        /// </param>
        /// <param name="canceled">
        /// Set to true if the wait was canceled via the <paramref
        /// name="cancellationToken"/>.
        /// </param>
        /// <returns>
        /// true if the <see cref="ManualResetEventAsync"/> was signaled within
        /// the allotted time; otherwise, false.
        /// </returns>
        /// <exception cref="T:System.ArgumentOutOfRangeException">
        /// <paramref name="millisecondsTimeout"/> is a negative number other
        /// than -1, which represents an infinite time-out.
        /// </exception>
        public bool Wait(
            int millisecondsTimeout,
            CancellationToken cancellationToken,
            out bool canceled)
        {
            bool ret = false;
            try
            {
                ret = GetWaitTask().Wait(millisecondsTimeout, cancellationToken);
                canceled = false;
            }
            catch (Exception ex)
                when (ex is OperationCanceledException
                    || (ex is AggregateException
                        && ex.InnerOf<OperationCanceledException>() != null))
            {
                canceled = true;
            }

            return ret;
        }
    }
}
