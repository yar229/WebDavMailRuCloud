using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using YaR.MailRuCloud.Api.Base.Requests.Types;
using YaR.MailRuCloud.Api.Links;

namespace YaR.MailRuCloud.Api.Base.Requests.Repo
{
    interface IRequestRepo
    {
        Task<bool> Login(Account.AuthCodeRequiredDelegate onAuthCodeRequired);
        void BanShardInfo(ShardInfo banShard);
        Task<ShardInfo> GetShardInfo(ShardType shardType);

        Task<CreateFolderResult> CreateFolder(string path);
        Task<AddFileResult> AddFile(string fileFullPath, string fileHash, FileSize fileSize, DateTime dateTime, ConflictResolver? conflictResolver);

        Task<AuthTokenResult> Auth();

        Task<CloneItemResult> CloneItem(string fromUrl, string toPath);

        Task<CopyResult> Copy(string sourceFullPath, string destinationPath, ConflictResolver? conflictResolver = null);

        Task<CopyResult> Move(string sourceFullPath, string destinationPath, ConflictResolver? conflictResolver = null);

        Task<IEntry> FolderInfo(string path, Link ulink, bool isWebLink = false, int offset = 0, int limit = int.MaxValue);

        Task<FolderInfoResult> ItemInfo(string path, bool isWebLink = false, int offset = 0, int limit = int.MaxValue);

        Task<AccountInfoResult> AccountInfo();

        Task<PublishResult> Publish(string fullPath);

        Task<UnpublishResult> Unpublish(string publicLink);

        Task<RemoveResult> Remove(string fullPath);

        Task<RenameResult> Rename(string fullPath, string newName);

        Task<Dictionary<ShardType, ShardInfo>> ShardInfo();


        string DownloadToken { get; }
    }
}
