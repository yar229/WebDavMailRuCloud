using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace YaR.MailRuCloud.Api
{
    public class MailRuShaEncoding
    {
        public string Sha1(Stream stream)
        {
            var sha1 = System.Security.Cryptography.SHA1.Create();
            sha1.Initialize();
            var initBuffer = Encoding.UTF8.GetBytes("mrCloud");
            sha1.TransformBlock(initBuffer, 0, initBuffer.Length, null, 0);

            byte[] buffer = new byte[8192];
            long length = 0;

            int read;
            while ((read = stream.Read(buffer, 0, buffer.Length)) > 0)
            {
                sha1.TransformBlock(buffer, 0, read, null, 0);
                length += read;
            }

            var finalBuffer = Encoding.UTF8.GetBytes(length.ToString());
            sha1.TransformBlock(finalBuffer, 0, finalBuffer.Length, null, 0);

            sha1.TransformFinalBlock(new byte[0], 0, 0);
            byte[] hash = sha1.Hash;
            string stringHash = BitConverter.ToString(hash);
            return stringHash;
        }
    }
}
