using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using YaR.MailRuCloud.Api.Base.Requests.Types;

namespace YaR.MailRuCloud.Api.Base.Requests.Repo
{
    class MixedRepo : IRequestRepo
    {
        private readonly IRequestRepo _webRepo;
        private readonly IRequestRepo _mobileRepo;

        public MixedRepo(CloudApi cloudApi)
        {
            _webRepo = new WebRequestRepo(cloudApi);
            _mobileRepo = new MobileRequestRepo(cloudApi);
        }

        public async Task<bool> Login(Account.AuthCodeRequiredDelegate onAuthCodeRequired)
        {
            return await _webRepo.Login(onAuthCodeRequired);
        }

        public void BanShardInfo(ShardInfo banShard)
        {
            _webRepo.BanShardInfo(banShard);
        }

        public async Task<ShardInfo> GetShardInfo(ShardType shardType)
        {
            return await _webRepo.GetShardInfo(shardType);
        }

        public async Task<CreateFolderResult> CreateFolder(string path)
        {
            return await _webRepo.CreateFolder(path);
        }

        public async Task<AddFileResult> AddFile(string fileFullPath, string fileHash, FileSize fileSize, DateTime dateTime, ConflictResolver? conflictResolver)
        {
            return await _mobileRepo.AddFile(fileFullPath, fileHash, fileSize, dateTime, conflictResolver);
        }

        public async Task<AuthTokenResult> Auth()
        {
            return await _webRepo.Auth();
        }

        public async Task<CloneItemResult> CloneItem(string fromUrl, string toPath)
        {
            return await _webRepo.CloneItem(fromUrl, toPath);
        }

        public async Task<CopyResult> Copy(string sourceFullPath, string destinationPath, ConflictResolver? conflictResolver = null)
        {
            return await _webRepo.Copy(sourceFullPath, destinationPath, conflictResolver);
        }

        public async Task<CopyResult> Move(string sourceFullPath, string destinationPath, ConflictResolver? conflictResolver = null)
        {
            return await _webRepo.Move(sourceFullPath, destinationPath, conflictResolver);
        }

        public async Task<FolderInfoResult> FolderInfo(string path, bool isWebLink = false, int offset = 0, int limit = Int32.MaxValue)
        {
            return await _webRepo.FolderInfo(path, isWebLink, offset, limit);
        }

        public async Task<FolderInfoResult> ItemInfo(string path, bool isWebLink = false, int offset = 0, int limit = Int32.MaxValue)
        {
            return await _webRepo.ItemInfo(path, isWebLink, offset, limit);
        }

        public async Task<AccountInfoResult> AccountInfo()
        {
            return await _webRepo.AccountInfo();
        }

        public async Task<PublishResult> Publish(string fullPath)
        {
            return await _webRepo.Publish(fullPath);
        }

        public async Task<UnpublishResult> Unpublish(string publicLink)
        {
            return await _webRepo.Unpublish(publicLink);
        }

        public async Task<RemoveResult> Remove(string fullPath)
        {
            return await _webRepo.Remove(fullPath);
        }

        public async Task<RenameResult> Rename(string fullPath, string newName)
        {
            return await _webRepo.Rename(fullPath, newName);
        }

        public async Task<Dictionary<ShardType, ShardInfo>> ShardInfo()
        {
            return await _webRepo.ShardInfo();
        }

        public string DownloadToken => _webRepo.DownloadToken;
    }
}