using System;
using System.IO;
using System.Security.Cryptography;

namespace YaR.Clouds.Base.Repos.YandexDisk.YadWebV2
{
    class YadHasher : ICloudHasher
    {
        public YadHasher()
        {
            _sha256.Initialize();
            _md5.Initialize();
            AppendInitBuffer();
        }

        public string Name => "yadsha256";

        public void Append(byte[] buffer, int offset, int length)
        {
            if (_isClosed)
                throw new Exception("Cannot append because hash already calculated.");

            _sha256.TransformBlock(buffer, offset, length, null, 0);
            _md5.TransformBlock(buffer, offset, length, null, 0);
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

        //public string HashString => BitConverter.ToString(Hash).Replace("-", string.Empty);

        public IFileHash Hash
        {
            get
            {
                if (null != _hashSha256 && null != _hashMd5) 
                    return new FileHashYad(_hashSha256, _hashMd5);

                AppendFinalBuffer();

                _sha256.TransformFinalBlock(Array.Empty<byte>(), 0, 0);
                _md5.TransformFinalBlock(Array.Empty<byte>(), 0, 0);
                _hashSha256 = _sha256.Hash;
                _hashMd5 = _md5.Hash;
                _isClosed = true;
                return new FileHashYad(_hashSha256, _hashMd5);
            }
        }

        public long Length => 20;

        private byte[] _hashSha256;
        private byte[] _hashMd5;

        private readonly SHA256 _sha256 = SHA256.Create();
        private readonly MD5 _md5 = MD5.Create();
        private bool _isClosed;

        private static void AppendInitBuffer()
        {
        }

        private static void AppendFinalBuffer()
        {
        }

        public void Dispose()
        {
            _sha256?.Dispose();
            _md5?.Dispose();
        }
    }
}
