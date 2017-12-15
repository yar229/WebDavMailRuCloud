using System;
using System.IO;
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
        HttpCommonSettings HttpSettings { get; }


        Stream GetDownloadStream(File file, long? start = null, long? end = null);
        HttpWebRequest UploadRequest(ShardInfo shard, File file, UploadMultipartBoundary boundary);

        //TODO: internal functionality, remove
        Task<ShardInfo> GetShardInfo(ShardType shardType);


        Task<IEntry> FolderInfo(string path, Link ulink, int offset = 0, int limit = int.MaxValue);

        Task<FolderInfoResult> ItemInfo(string path, bool isWebLink = false, int offset = 0, int limit = int.MaxValue);

        Task<AccountInfoResult> AccountInfo();



        Task<CreateFolderResult> CreateFolder(string path);

        Task<AddFileResult> AddFile(string fileFullPath, string fileHash, FileSize fileSize, DateTime dateTime, ConflictResolver? conflictResolver);

        Task<CloneItemResult> CloneItem(string fromUrl, string toPath);

        Task<CopyResult> Copy(string sourceFullPath, string destinationPath, ConflictResolver? conflictResolver = null);

        Task<CopyResult> Move(string sourceFullPath, string destinationPath, ConflictResolver? conflictResolver = null);

        Task<PublishResult> Publish(string fullPath);

        Task<UnpublishResult> Unpublish(string publicLink);

        Task<RemoveResult> Remove(string fullPath);

        Task<RenameResult> Rename(string fullPath, string newName);
    }
}
