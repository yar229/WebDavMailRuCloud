using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;

namespace MailRuCloudApi
{
    [Serializable]
    public abstract class ReturnedInfo
    {
    }

    class CarotCryptStream : Stream
    {
        //private EncryptFs pFs;
        private Stream _pStream;
        private readonly ICryptoTransform _pTransform;
        private readonly CryptoStreamMode _pMode;
        private readonly SHA256Managed _pSha;
        private long _pLength;
        private long _pPosition;
        private readonly byte[] _pReadBuf;
        private byte[] _pDecryptBuf;

        public override bool CanRead => _pMode == CryptoStreamMode.Read;

        public override bool CanWrite => _pMode == CryptoStreamMode.Write;

        public override bool CanSeek => false;

        public override long Length => _pLength;

        public override long Position
        {
            get
            {
                return _pPosition;
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public CarotCryptStream(Stream basestream, ICryptoTransform transformer, long length, bool fooImWriteStream)
        {
            _pTransform = transformer;
            _pMode = CryptoStreamMode.Write;
            _pSha = new SHA256Managed();
            _pLength = length;
            _pPosition = 0L;
            byte[] buffer = new byte[64];
            Encoding.UTF8.GetBytes("CarotDAV Encryption 1.0 ").CopyTo(buffer, 0);
            basestream.Write(buffer, 0, buffer.Length);
            CloseNotifyStream closeNotifyStream = new CloseNotifyStream(basestream);
            closeNotifyStream.ClosingEvent += Stream_Closing;
            closeNotifyStream.ClosedEvent += Stream_Closed;
            _pStream = new CryptoStream(closeNotifyStream, _pTransform, _pMode);
        }

        public CarotCryptStream(Stream basestream, ICryptoTransform transformer, long length)
        {
            _pTransform = transformer;
            _pMode = CryptoStreamMode.Read;
            _pSha = new SHA256Managed();
            _pLength = length;
            _pPosition = 0L;
            _pStream = basestream;
            byte[] numArray = new byte[144];
            int offset = 0;
            while (offset < numArray.Length)
            {
                int num = _pStream.Read(numArray, offset, checked(numArray.Length - offset));
                if (num == 0)
                    throw new InconsistencyDetectedException(new Uri(""), "Encrypted file broken");
                checked { offset += num; }
            }
            if (!Encoding.UTF8.GetString(numArray, 0, 24).Equals("CarotDAV Encryption 1.0 "))
                throw new InconsistencyDetectedException(new Uri(""), "Encrypted file broken");
            _pReadBuf = new byte[80];
            Array.Copy(numArray, 64, _pReadBuf, 0, _pReadBuf.Length);
            _pDecryptBuf = new byte[0];
        }

        public override void Flush()
        {
            _pStream.Flush();
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotImplementedException();
        }

        public override void SetLength(long value)
        {
            throw new NotImplementedException();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            if (_pMode != CryptoStreamMode.Read)
                throw new InvalidOperationException();
            if (_pStream == null)
                throw new ObjectDisposedException(GetType().FullName);
            if (_pLength >= 0L && checked(_pPosition + count) > _pLength)
                count = checked((int)(_pLength - _pPosition));
            if (count < 0)
                throw new ArgumentOutOfRangeException(nameof(count));
            if (count == 0)
                return 0;
            int num1 = 0;
            label_11:
            int num2 = count;
            if (num2 > _pDecryptBuf.Length)
                num2 = _pDecryptBuf.Length;
            Array.Copy(_pDecryptBuf, 0, buffer, offset, num2);
            checked
            {
                count -= num2;
                offset += num2; 
                num1 += num2; 
            }
            _pPosition = checked(_pPosition + num2);
            byte[] numArray1 = new byte[checked(_pDecryptBuf.Length - num2 - 1 + 1)];
            Array.Copy(_pDecryptBuf, num2, numArray1, 0, checked(_pDecryptBuf.Length - num2));
            _pDecryptBuf = numArray1;
            if (count == 0)
                return num1;
            int num3 = checked(count + 15) & -16;
            byte[] numArray2 = new byte[checked(num3 - 1 + 1)];
            int num4 = 0;
            while (num4 < numArray2.Length)
            {
                int num5 = _pStream.Read(numArray2, num4, checked(numArray2.Length - num4));
                if (num5 == 0)
                {
                    byte[] inputBuffer = new byte[checked(_pReadBuf.Length + num4 - 1 + 1)];
                    Array.Copy(_pReadBuf, 0, inputBuffer, 0, _pReadBuf.Length);
                    Array.Copy(numArray2, 0, inputBuffer, _pReadBuf.Length, num4);
                    byte[] numArray3 = new byte[64];
                    Array.Copy(inputBuffer, checked(inputBuffer.Length - numArray3.Length), numArray3, 0, numArray3.Length);
                    byte[] numArray4 = _pTransform.TransformFinalBlock(inputBuffer, 0, checked(num4 + 15) & -16);
                    _pDecryptBuf = new byte[checked(num4 - 1 + 1)];
                    Array.Copy(numArray4, 0, _pDecryptBuf, 0, _pDecryptBuf.Length);
                    _pSha.TransformFinalBlock(_pDecryptBuf, 0, _pDecryptBuf.Length);
                    string s = "";
                    int num6 = 0;
                    int num7 = checked(_pSha.Hash.Length - 1);
                    int index1 = num6;
                    while (index1 <= num7)
                    {
                        s += _pSha.Hash[index1].ToString("X2");
                        checked { ++index1; }
                    }
                    byte[] bytes = Encoding.UTF8.GetBytes(s);
                    if (bytes.Length != numArray3.Length)
                        throw new IOException("File hash doesn't match.");
                    int num8 = 0;
                    int num9 = checked(bytes.Length - 1);
                    int index2 = num8;
                    while (index2 <= num9)
                    {
                        if (bytes[index2] != numArray3[index2])
                            throw new IOException("File hash doesn't match.");
                        checked { ++index2; }
                    }
                    if (_pLength < 0L)
                        _pLength = checked(_pPosition + num4);
                    else if (_pLength > checked(_pPosition + num4))
                        throw new InvalidRangeException();
                    if (count > num4)
                    {
                        count = num4;
                    }
                    goto label_11;
                }
                checked { num4 += num5; }
            }
            byte[] numArray5 = new byte[checked(num3 - 1 + 1)];
            if (num3 < _pReadBuf.Length)
            {
                numArray5 = new byte[checked(num3 - 1 + 1)];
                _pTransform.TransformBlock(_pReadBuf, 0, num3, numArray5, 0);
                Array.Copy(_pReadBuf, num3, _pReadBuf, 0, checked(_pReadBuf.Length - num3));
                Array.Copy(numArray2, 0, _pReadBuf, checked(_pReadBuf.Length - num3), num3);
            }
            else
            {
                _pTransform.TransformBlock(_pReadBuf, 0, _pReadBuf.Length, numArray5, 0);
                _pTransform.TransformBlock(numArray2, 0, checked(numArray2.Length - _pReadBuf.Length), numArray5, _pReadBuf.Length);
                Array.Copy(numArray2, checked(numArray2.Length - _pReadBuf.Length), _pReadBuf, 0, _pReadBuf.Length);
            }
            _pSha.TransformBlock(numArray5, 0, numArray2.Length, null, 0);
            Array.Copy(numArray5, 0, buffer, offset, count);
            _pPosition = checked(_pPosition + count);
            int num10 = checked(num1 + count);
            _pDecryptBuf = new byte[checked(num3 - count - 1 + 1)];
            Array.Copy(numArray5, count, _pDecryptBuf, 0, _pDecryptBuf.Length);
            return num10;
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            if (_pStream == null)
                throw new ObjectDisposedException(GetType().FullName);
            if (_pMode != CryptoStreamMode.Write)
                throw new InvalidOperationException();
            if (_pLength >= 0L && checked(_pPosition + count) > _pLength)
                throw new InvalidRangeException();
            if (count == 0)
                return;
            _pSha.TransformBlock(buffer, offset, count, null, 0);
            _pStream.Write(buffer, offset, count);
            _pPosition = checked(_pPosition + count);
        }

        protected override void Dispose(bool disposing)
        {
            try
            {
                if (!disposing)
                    return;
                switch (_pMode)
                {
                    case CryptoStreamMode.Read:
                        if (_pStream == null)
                            break;
                        _pStream.Close();
                        _pStream = null;
                        break;
                    case CryptoStreamMode.Write:
                        if (_pStream == null)
                            break;
                        _pStream.Close();
                        _pStream = null;
                        break;
                    default:
                        throw new Exception();
                }
            }
            finally
            {
                base.Dispose(disposing);
            }
        }

        private void Stream_Closing(object sender, EventArgs e)
        {
            if (_pMode != CryptoStreamMode.Write)
                throw new InvalidOperationException();
            CloseNotifyStream closeNotifyStream = (CloseNotifyStream)sender;
            int num1 = checked((int)(_pPosition % _pTransform.OutputBlockSize));
            if (num1 == 0)
                num1 = 16;
            byte[] buffer = new byte[checked(num1 - 1 + 1)];
            closeNotifyStream.Write(buffer, 0, buffer.Length);
            _pSha.TransformFinalBlock(new byte[0], 0, 0);
            string s = "";
            int num2 = 0;
            int num3 = checked(_pSha.Hash.Length - 1);
            int index = num2;
            while (index <= num3)
            {
                s += _pSha.Hash[index].ToString("X2");
                checked { ++index; }
            }
            byte[] bytes = Encoding.UTF8.GetBytes(s);
            if (bytes.Length != 64)
                throw new IOException();
            closeNotifyStream.Write(bytes, 0, bytes.Length);
        }

        private void Stream_Closed(object sender, EventArgs e)
        {
            //if (_pMode != CryptoStreamMode.Write)
            //    throw new InvalidOperationException();
            //ReturnedInfo returnedInfo = this.pFs.basefs.CloseWrite(((CloseNotifyStream)sender).BaseStream);
            //ResourceInfo ri = returnedInfo as ResourceInfo;
            //if (ri != null)
            //{
            //    this.pFs.CreateInfo2(pUri, ri, true);
            //    _pReturnedInfo = (ReturnedInfo)ri;
            //}
            //else
            //    _pReturnedInfo = (ReturnedInfo)new EncryptFs.EncryptId(pUri, returnedInfo as ResourceId, true, this.pFs);
            //if (_pLength >= 0L && _pPosition != _pLength)
            //    throw new InvalidRangeException();
        }
    }


    [Serializable]
    public class InvalidRangeException : Exception
    {
        public InvalidRangeException(string message, Exception innerexception)
          : base(message, innerexception)
        {
        }

        public InvalidRangeException(Exception innerexception)
          : this("Invalid Range", innerexception)
        {
        }

        public InvalidRangeException(string message)
          : this(message, null)
        {
        }

        public InvalidRangeException()
          : this("Invalid Range", null)
        {
        }
    }

    [Serializable]
    public class InconsistencyDetectedException : Exception
    {
        public Uri TargetUri { get; }

        public InconsistencyDetectedException(Uri uri, string message)
          : base(message)
        {
            TargetUri = uri;
        }

        public InconsistencyDetectedException(Uri uri, Exception innerexception)
          : base("Inconsistency detected. Remote resource was changed during atomic operation.", innerexception)
        {
            TargetUri = uri;
        }

        public InconsistencyDetectedException(Uri uri, string message, Exception innerexception)
          : base(message, innerexception)
        {
            TargetUri = uri;
        }
    }

    public class CloseNotifyStream : Stream
    {
        private readonly Stream _pBaseStream;
        private long _pTotalWrite;
        public object Tag;
        private bool _pDisposed;

        public Stream BaseStream => _pBaseStream;

        public long TotalWrite => _pTotalWrite;

        public override bool CanRead => _pBaseStream.CanRead;

        public override bool CanSeek => _pBaseStream.CanSeek;

        public override bool CanWrite => _pBaseStream.CanWrite;

        public override bool CanTimeout => _pBaseStream.CanTimeout;

        public override long Length => _pBaseStream.Length;

        public override long Position
        {
            get
            {
                return _pBaseStream.Position;
            }
            set
            {
                _pBaseStream.Position = value;
            }
        }

        public override int ReadTimeout
        {
            get
            {
                return _pBaseStream.ReadTimeout;
            }
            set
            {
                _pBaseStream.ReadTimeout = value;
            }
        }

        public override int WriteTimeout
        {
            get
            {
                return _pBaseStream.WriteTimeout;
            }
            set
            {
                _pBaseStream.WriteTimeout = value;
            }
        }

        public event EventHandler ClosedEvent;

        public event EventHandler ClosingEvent;

        public CloseNotifyStream(Stream basestream)
        {
            _pDisposed = false;
            _pBaseStream = basestream;
            _pTotalWrite = 0L;
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            return _pBaseStream.Read(buffer, offset, count);
        }

        public override IAsyncResult BeginRead(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
        {
            return base.BeginRead(buffer, offset, count, callback, RuntimeHelpers.GetObjectValue(state));
        }

        public override int EndRead(IAsyncResult asyncResult)
        {
            return base.EndRead(asyncResult);
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            return _pBaseStream.Seek(offset, origin);
        }

        public override void SetLength(long value)
        {
            _pBaseStream.SetLength(value);
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            _pTotalWrite = checked(_pTotalWrite + count);
            _pBaseStream.Write(buffer, offset, count);
        }

        public override IAsyncResult BeginWrite(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
        {
            _pTotalWrite = checked(_pTotalWrite + count);
            return base.BeginWrite(buffer, offset, count, callback, RuntimeHelpers.GetObjectValue(state));
        }

        public override void EndWrite(IAsyncResult asyncResult)
        {
            base.EndWrite(asyncResult);
        }

        public override void Flush()
        {
            _pBaseStream.Flush();
        }

        protected override void Dispose(bool disposing)
        {
            try
            {
                if (!disposing || _pDisposed)
                    return;
                _pDisposed = true;
                try
                {
                    EventHandler closingEventEvent = ClosingEvent;
                    closingEventEvent?.Invoke(this, EventArgs.Empty);
                }
                finally
                {
                    _pBaseStream.Close();
                }
                EventHandler closedEventEvent = ClosedEvent;
                if (closedEventEvent == null)
                    return;
                closedEventEvent(this, EventArgs.Empty);
            }
            finally
            {
                base.Dispose(disposing);
            }
        }
    }
}
