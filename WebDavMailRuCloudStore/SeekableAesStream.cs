using System;
using System.IO;
using System.Security.Cryptography;

namespace YaR.WebDavMailRu.CloudStore
{
    /// <summary>
    /// http://stackoverflow.com/questions/5026409/how-to-add-seek-and-position-capabilities-to-cryptostream/38974483#38974483
    /// 
    /// Usage:
    /// static void test()
    ///    {
    ///        var buf = new byte[255];
    ///        for (byte i = 0; i &lt; buf.Length; i++)
    ///            buf[i] = i;
    ///
    ///        //encrypting
    ///        var uniqueSalt = new byte[16]; //** WARNING **: MUST be unique for each stream otherwise there is NO security
    ///        var baseStream = new MemoryStream();
    ///        var cryptor = new SeekableAesStream(baseStream, "password", uniqueSalt);
    ///        cryptor.Write(buf, 0, buf.Length);
    ///
    ///        //decrypting at position 200
    ///        cryptor.Position = 200;
    ///        var decryptedBuffer = new byte[50];
    ///        cryptor.Read(decryptedBuffer, 0, 50);
    ///
    ///    }
    /// </summary>

    public class SeekableAesStream : Stream
    {
        private readonly Stream _baseStream;
        private readonly AesManaged _aes;
        private readonly ICryptoTransform _encryptor;
        public bool AutoDisposeBaseStream { get; set; } = true;

        /// <param name="salt">//** WARNING **: MUST be unique for each stream otherwise there is NO security</param>
        public SeekableAesStream(Stream baseStream, string password, byte[] salt)
        {
            _baseStream = baseStream;
            using (var key = new PasswordDeriveBytes(password, salt))
            {
                _aes = new AesManaged
                {
                    KeySize = 128,
                    Mode = CipherMode.ECB,
                    Padding = PaddingMode.None
                };
                _aes.Key =  key.GetBytes(_aes.KeySize / 8);
                _aes.IV = new byte[16]; //zero buffer is adequate since we have to use new salt for each stream
                _encryptor = _aes.CreateEncryptor(_aes.Key, _aes.IV);
            }
        }

        private void Cipher(byte[] buffer, int offset, int count, long streamPos)
        {
            //find block number
            var blockSizeInByte = _aes.BlockSize / 8;
            var blockNumber = (streamPos / blockSizeInByte) + 1;
            var keyPos = streamPos % blockSizeInByte;

            //buffer
            var outBuffer = new byte[blockSizeInByte];
            var nonce = new byte[blockSizeInByte];
            var init = false;

            for (int i = offset; i < count; i++)
            {
                //encrypt the nonce to form next xor buffer (unique key)
                if (!init || (keyPos % blockSizeInByte) == 0)
                {
                    BitConverter.GetBytes(blockNumber).CopyTo(nonce, 0);
                    _encryptor.TransformBlock(nonce, 0, nonce.Length, outBuffer, 0);
                    if (init) keyPos = 0;
                    init = true;
                    blockNumber++;
                }
                buffer[i] ^= outBuffer[keyPos]; //simple XOR with generated unique key
                keyPos++;
            }
        }

        public override bool CanRead => _baseStream.CanRead;
        public override bool CanSeek => _baseStream.CanSeek;
        public override bool CanWrite => _baseStream.CanWrite;
        public override long Length => _baseStream.Length;
        public override long Position { get { return _baseStream.Position; } set { _baseStream.Position = value; } }
        public override void Flush() { _baseStream.Flush(); }
        public override void SetLength(long value) { _baseStream.SetLength(value); }
        public override long Seek(long offset, SeekOrigin origin) { return _baseStream.Seek(offset, origin); }

        public override int Read(byte[] buffer, int offset, int count)
        {
            var streamPos = Position;
            var ret = _baseStream.Read(buffer, offset, count);
            Cipher(buffer, offset, count, streamPos);
            return ret;
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            Cipher(buffer, offset, count, Position);
            _baseStream.Write(buffer, offset, count);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _encryptor?.Dispose();
                _aes?.Dispose();
                if (AutoDisposeBaseStream)
                    _baseStream?.Dispose();
            }

            base.Dispose(disposing);
        }
    }
}
