using System.IO;
using System.Linq;
using System.Text;
using YaR.MailRuCloud.Api.XTSSharp;
using YaR.WebDavMailRu.CloudStore.XTSSharp;

namespace YaR.MailRuCloud.Api.Base
{
    public class DownloadStreamFabric
    {
        private readonly MailRuCloud _cloud;

        public DownloadStreamFabric(MailRuCloud cloud)
        {
            _cloud = cloud;
        }

        public Stream Create(File file, long? start = null, long? end = null)
        {
            if (file.ServiceInfo.IsCrypted)
                return CreateXTSStream(file, start, end);

            return new DownloadStream(file, _cloud.CloudApi, start, end);
        }

        private Stream CreateXTSStream(File file, long? start = null, long? end = null)
        {
            var pub = CryptoUtil.GetCryptoPublicInfo(_cloud, file);
            var key = CryptoUtil.GetCryptoKey(_cloud.CloudApi.Account.Credentials.PasswordCrypt, pub.Salt);
            var xts = XtsAes256.Create(key, pub.IV);

            long fileLength = file.OriginalSize;
            long requestedOffset = start ?? 0;
            long requestedEnd = end ?? fileLength;
            //long requestedLength = requestedEnd - requestedOffset;

            long alignedOffset = requestedOffset / XTSSectorSize * XTSSectorSize;
            long alignedEnd = requestedEnd % XTSBlockSize == 0
                ? requestedEnd
                : (requestedEnd / XTSBlockSize + 1) * XTSBlockSize;

            if (alignedEnd == 0) alignedEnd = 16;
            //long alignedLength = alignedEnd - alignedOffset;

            var downStream = new DownloadStream(file, _cloud.CloudApi, alignedOffset, alignedEnd);
            var xtsStream = new XTSReadOnlyStream(downStream, xts, XTSSectorSize, 0, file.ServiceInfo.CryptInfo.AlignBytes); // (int)(alignedOffset - requestedOffset)

            return xtsStream;
        }

        public const int XTSSectorSize = 512;
        public const long XTSBlockSize = XTSWriteOnlyStream.BlockSize; // 0x0FL
    }
}