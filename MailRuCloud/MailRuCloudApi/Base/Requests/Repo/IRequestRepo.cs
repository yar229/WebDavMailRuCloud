using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using YaR.MailRuCloud.Api.Base.Requests.Types;
using YaR.MailRuCloud.Api.Base.Threads;
using YaR.MailRuCloud.Api.Links;

namespace YaR.MailRuCloud.Api.Base.Requests.Repo
{
    interface IRequestRepo
    {

        IAuth Authent { get; }

        IWebProxy Proxy { get; }


        void BanShardInfo(ShardInfo banShard);
        Task<ShardInfo> GetShardInfo(ShardType shardType);

        Task<CreateFolderResult> CreateFolder(string path);
        Task<AddFileResult> AddFile(string fileFullPath, string fileHash, FileSize fileSize, DateTime dateTime, ConflictResolver? conflictResolver);

        


        HttpWebRequest UploadRequest(ShardInfo shard, File file, UploadMultipartBoundary boundary);
        HttpWebRequest DownloadRequest(long instart, long inend, File file, ShardInfo shard);

        Task<CloneItemResult> CloneItem(string fromUrl, string toPath);

        Task<CopyResult> Copy(string sourceFullPath, string destinationPath, ConflictResolver? conflictResolver = null);

        Task<CopyResult> Move(string sourceFullPath, string destinationPath, ConflictResolver? conflictResolver = null);

        Task<IEntry> FolderInfo(string path, Link ulink, int offset = 0, int limit = int.MaxValue);

        Task<FolderInfoResult> ItemInfo(string path, bool isWebLink = false, int offset = 0, int limit = int.MaxValue);

        Task<AccountInfoResult> AccountInfo();

        Task<PublishResult> Publish(string fullPath);

        Task<UnpublishResult> Unpublish(string publicLink);

        Task<RemoveResult> Remove(string fullPath);

        Task<RenameResult> Rename(string fullPath, string newName);

        //Task<Dictionary<ShardType, ShardInfo>> ShardInfo();
        
    }
}
