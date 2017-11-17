using System;
using System.IO;
using System.Linq;
using System.Text;
using YaR.MailRuCloud.Api.XTSSharp;
using YaR.WebDavMailRu.CloudStore.XTSSharp;

namespace YaR.MailRuCloud.Api.Base
{
    public class DownloadStreamFabric
    {
        private readonly CloudApi _cloud;

        public DownloadStreamFabric(CloudApi cloud)
        {
            _cloud = cloud;
        }

        public Stream Create(File file, long? start = null, long? end = null)
        {
            if (file.ServiceInfo.IsCrypted)
                return CreateXTSStream(file, start, end);

            return new DownloadStream(file, _cloud, start, end);
        }

        private Stream CreateXTSStream(File file, long? start = null, long? end = null)
        {
            // just testing crypt
            var key1 = new byte[32];
            var key2 = new byte[32];
            Array.Copy(Encoding.ASCII.GetBytes("01234567890123456789012345678900zzzzzzzzzzzzzzzzzzzzzz"), key1, 32);
            Array.Copy(Encoding.ASCII.GetBytes("01234567890123456789012345678900zzzzzzzzzzzzzzzzzzzzzz"), key2, 32);
            var xts = XtsAes256.Create(key1, key2);

            long fileLength = file.OriginalSize; //Parts.Sum(f => f.Size);
            long requestedOffset = start ?? 0;
            long requestedEnd = end ?? fileLength;
            long requestedLength = requestedEnd - requestedOffset;

            long alignedOffset = requestedOffset / XTSSectorSize * XTSSectorSize;
            long alignedEnd = requestedEnd % XTSBlockSize == 0
                ? requestedEnd
                : (requestedEnd / XTSBlockSize + 1) * XTSBlockSize;
            long alignedLength = alignedEnd - alignedOffset;
            

            var downStream = new DownloadStream(file, _cloud, alignedOffset, alignedEnd);
            var xtsStream = new XTSReadOnlyStream(downStream, xts, XTSSectorSize,
                (int) (alignedOffset - requestedOffset), file.ServiceInfo.CryptInfo.AlignBytes); //(int)(alignedLength - requestedLength));

            return xtsStream;
            //return downStream;
        }

        public const int XTSSectorSize = 512;
        public const long XTSBlockSize = XTSWriteOnlyStream.BlockSize; // 0x0FL
    }
}