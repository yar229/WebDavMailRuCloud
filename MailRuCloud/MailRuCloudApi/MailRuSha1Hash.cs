using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace YaR.MailRuCloud.Api
{
    public class MailRuSha1Hash
    {
        public MailRuSha1Hash()
        {
            _sha1.Initialize();
            AppendInitBuffer();
        }

        public void Append(byte[] buffer, int pos, int length)
        {
            if (_isClosed)
                throw new Exception("Cannot append because MRSHA1 already calculated.");

            _sha1.TransformBlock(buffer, pos, length, null, 0);
            _length += length;
        }

        public void Append(byte[] buffer)
        {
            Append(buffer, 0, buffer.Length);
        }

        public void Append(Stream stream)
        {
            if (_isClosed)
                throw new Exception("Cannot append because MRSHA1 already calculated.");

            byte[] buffer = new byte[8192];
            int read;
            while ((read = stream.Read(buffer, 0, buffer.Length)) > 0)
            {
                _sha1.TransformBlock(buffer, 0, read, null, 0);
                _length += read;
            }
        }

        public string HashString => BitConverter.ToString(Hash).Replace("-", string.Empty);

        public byte[] Hash
        {
            get
            {
                if (null == _hash)
                {
                    AppendFinalBuffer();

                    _sha1.TransformFinalBlock(new byte[0], 0, 0);
                    _hash = _sha1.Hash;
                    _isClosed = true;
                }
                return _hash;
            }
        }

        private byte[] _hash;

        private readonly SHA1 _sha1 = SHA1.Create();
        private long _length;
        private bool _isClosed;

        private void AppendInitBuffer()
        {
            var initBuffer = Encoding.UTF8.GetBytes("mrCloud");
            _sha1.TransformBlock(initBuffer, 0, initBuffer.Length, null, 0);
        }

        private void AppendFinalBuffer()
        {
            var finalBuffer = Encoding.UTF8.GetBytes(_length.ToString());
            _sha1.TransformBlock(finalBuffer, 0, finalBuffer.Length, null, 0);
        }
    }
}
