using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using YaR.Clouds.Base.Requests;
using YaR.Clouds.Base.Requests.Types;
using YaR.Clouds.Base.Streams;
using YaR.Clouds.Common;

namespace YaR.Clouds.Base.Repos
{
    public interface IRequestRepo
    {
        IAuth Authent { get; }
        HttpCommonSettings HttpSettings { get; }


        Stream GetDownloadStream(File file, long? start = null, long? end = null);

        Task<UploadFileResult> DoUpload(HttpClient client, PushStreamContent content, File file);

        Task<IEntry> FolderInfo(RemotePath path, int offset = 0, int limit = int.MaxValue, int depth = 1);

        Task<FolderInfoResult> ItemInfo(RemotePath path, int offset = 0, int limit = int.MaxValue);

        Task<AccountInfoResult> AccountInfo();

        Task<CreateFolderResult> CreateFolder(string path);

        Task<AddFileResult> AddFile(string fileFullPath, IFileHash fileHash, FileSize fileSize, DateTime dateTime, ConflictResolver? conflictResolver);

        Task<CloneItemResult> CloneItem(string fromUrl, string toPath);

        Task<CopyResult> Copy(string sourceFullPath, string destinationPath, ConflictResolver? conflictResolver = null);

        Task<CopyResult> Move(string sourceFullPath, string destinationPath, ConflictResolver? conflictResolver = null);

        Task<PublishResult> Publish(string fullPath);

        Task<UnpublishResult> Unpublish(Uri publicLink, string fullPath);

        Task<RemoveResult> Remove(string fullPath);

        Task<RenameResult> Rename(string fullPath, string newName);

        //TODO: move to inner repo functionality
        Dictionary<ShardType, ShardInfo> GetShardInfo1();

        IEnumerable<PublicLinkInfo> GetShareLinks(string path);

        void CleanTrash();

        //TODO: bad quick patch
        string ConvertToVideoLink(Uri publicLink, SharedVideoResolution videoResolution);

        ICloudHasher GetHasher();

        bool SupportsAddSmallFileByHash { get; }
        bool SupportsDeduplicate { get; }

        string PublicBaseUrlDefault { get; }
    }
}
