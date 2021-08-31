using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace YaR.Clouds.Base.Repos.MailRuCloud
{
    public class MailRuSha1Hash : ICloudHasher
    {
        public MailRuSha1Hash()
        {
            _sha1.Initialize();
            AppendInitBuffer();
        }

        public string Name => "mrcsha1";

        public void Append(byte[] buffer, int offset, int length)
        {
            if (_isClosed)
                throw new Exception("Cannot append because MRSHA1 already calculated.");

            if (_length < 20)
                Array.Copy(buffer, offset, _smallContent, _length, Math.Min(length, 20 - _length));

            _sha1.TransformBlock(buffer, offset, length, null, 0);
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

            var buffer = new byte[8192];
            int read;
            while ((read = stream.Read(buffer, 0, buffer.Length)) > 0)
            {
                Append(buffer, 0, read);
            }
        }

        public string HashString => Hash.Hash.Value; //BitConverter.ToString(Hash).Replace("-", string.Empty);

        public IFileHash Hash
        {
            get
            {
                if (null == _hash)
                {
                    if (_length <= 20)
                        _hash = _smallContent;
                    else
                    {
                        AppendFinalBuffer();

                        _sha1.TransformFinalBlock(Array.Empty<byte>(), 0, 0);
                        _hash = _sha1.Hash;
                    }
                    _isClosed = true;
                }
                return new FileHashMrc(_hash);
            }
        }

        public long Length => 20;

        private byte[] _hash;

        private readonly byte[] _smallContent = new byte[20];
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

        public void Dispose()
        {
            _sha1?.Dispose();
        }
    }
}
