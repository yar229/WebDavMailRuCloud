using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using YaR.MailRuCloud.Api.XTSSharp;
using YaR.WebDavMailRu.CloudStore.XTSSharp;

namespace YaR.MailRuCloud.Api.Base
{
    public class UploadStreamFabric
    {
        private readonly MailRuCloud _cloud;

        public UploadStreamFabric(MailRuCloud cloud)
        {
            _cloud = cloud;
        }

        public async Task<Stream> Create(File file, FileUploadedDelegate onUploaded = null, bool discardEncryption = false)
        {
            var folder = await _cloud.GetItem(file.Path, MailRuCloud.ItemType.Folder) as Folder;
            if (null == folder)
                throw new DirectoryNotFoundException(file.Path);

            if (folder.CryptRequired && !discardEncryption)
            {
                var info = CryptoUtil.GetCryptoKeyAndSalt(_cloud.CloudApi.Account.Credentials.PasswordCrypt);
                var xts = XtsAes256.Create(info.Key, info.IV);

                file.ServiceInfo.CryptInfo = new CryptInfo
                {
                    PublicKey = new CryptoKeyInfo{Salt = info.Salt, IV = info.IV},
                    AlignBytes = (uint) (file.Size % XTSWriteOnlyStream.BlockSize != 0 ? XTSWriteOnlyStream.BlockSize - file.Size % XTSWriteOnlyStream.BlockSize : 0)
                };

                var size = file.Size % XTSWriteOnlyStream.BlockSize == 0
                    ? file.Size.DefaultValue
                    : (file.Size / XTSWriteOnlyStream.BlockSize + 1) * XTSWriteOnlyStream.BlockSize;

                var ustream = new SplittedUploadStream(file.FullPath, _cloud, size, false, file.ServiceInfo.CryptInfo);
                if (onUploaded != null) ustream.FileUploaded += onUploaded;
                var encustream = new XTSWriteOnlyStream(ustream, xts, XTSWriteOnlyStream.DefaultSectorSize);

                return encustream;
            }

            var stream = new SplittedUploadStream(file.FullPath, _cloud, file.Size);
            if (onUploaded != null) stream.FileUploaded += onUploaded;

            return stream;
        }
    }
}