using System;
using System.IO;
using System.Security.Cryptography;

namespace YaR.Clouds.Base.Repos.YandexDisk.YadWeb
{
    class YadHasher : ICloudHasher
    {
        public YadHasher()
        {
            _sha256.Initialize();
            AppendInitBuffer();
        }

        public void Append(byte[] buffer, int offset, int length)
        {
            if (_isClosed)
                throw new Exception("Cannot append because MRSHA1 already calculated.");

            _sha256.TransformBlock(buffer, offset, length, null, 0);
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
                Append(buffer, 0, read);
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

                    _sha256.TransformFinalBlock(new byte[0], 0, 0);
                    _hash = _sha256.Hash;
                    _isClosed = true;
                }
                return _hash;
            }
        }

        public long Length => 20;

        private byte[] _hash;

        private readonly SHA256 _sha256 = SHA256.Create();
        private bool _isClosed;

        private void AppendInitBuffer()
        {
        }

        private void AppendFinalBuffer()
        {
        }

        public void Dispose()
        {
            _sha256?.Dispose();
        }
    }
}
