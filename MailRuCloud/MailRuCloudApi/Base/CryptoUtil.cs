using System;
using System.Security.Cryptography;

namespace YaR.Clouds.Base
{
    public static class CryptoUtil
    {
        public static byte[] GetCryptoKey(string password, byte[] salt)
        {
            using var keygen = new Rfc2898DeriveBytes(password, salt, 4002);
            var key = keygen.GetBytes(32);
            return key;
        }

        public static KeyAndSalt GetCryptoKeyAndSalt(string password, int saltSize = SaltSizeInBytes)
        {
            using var keygen = new Rfc2898DeriveBytes(password, saltSize, 4002);
            var res = new KeyAndSalt
            {
                Salt = keygen.Salt,
                Key = keygen.GetBytes(32),
                IV = keygen.GetBytes(32)
            };
            return res;
        }

        public static CryptoKeyInfo GetCryptoPublicInfo(Cloud cloud, File file)
        {
            var iv = file.EnsurePublicKey(cloud);
            if (null == iv)
                throw new Exception("Cannot get crypto public key");
            return iv;
        }

        //public static byte[] CreateSalt(int saltSizeInBytes = SaltSizeInBytes)
        //{
        //    byte[] bytesSalt = new byte[saltSizeInBytes];
        //    using (RNGCryptoServiceProvider crypto = new RNGCryptoServiceProvider())
        //    {
        //        crypto.GetBytes(bytesSalt);
        //        return bytesSalt;
        //    }
        //}


        public const int SaltSizeInBytes = 8;

        public class KeyAndSalt
        {
            public byte[] Key { get; set; }
            public byte[] Salt { get; set; }
            public byte[] IV { get; set; }
        }
    }
}