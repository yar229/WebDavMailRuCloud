using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using YaR.Clouds.Base.Requests;
using YaR.Clouds.Base.Requests.Types;
using YaR.Clouds.Base.Streams;
using YaR.Clouds.Common;
using YaR.Clouds.Links;

namespace YaR.Clouds.Base.Repos
{
    public class RemotePath
    {
        private RemotePath()
        {}

        public static RemotePath Get(string path) => new RemotePath{Path = path};
        public static RemotePath Get(Link link) => new RemotePath{Link = link};
        //public static RemotePath Get(string path, Link link)
        //{
        //    if (string.IsNullOrEmpty(path) && null == link)
        //        throw new ArgumentException("cannot create empty RemotePath");
        //    if (!string.IsNullOrEmpty(path) && null != link)
        //        throw new ArgumentException("cannot create RemotePath with path and link");

        //    return new RemotePath {Link = link, Path = path};
        //}


        public string Path { get; private set; }
        public Link Link { get; private set;}

        public bool IsLink => Link != null;
    }

    public interface IRequestRepo
    {
        IAuth Authent { get; }
        HttpCommonSettings HttpSettings { get; }


        Stream GetDownloadStream(File file, long? start = null, long? end = null);

        //HttpWebRequest UploadRequest(ShardInfo shard, File file, UploadMultipartBoundary boundary);
        //[Obsolete] HttpWebRequest UploadRequest(File file, UploadMultipartBoundary boundary);
        //HttpRequestMessage UploadClientRequest(PushStreamContent content, File file);
        Task<UploadFileResult> DoUpload(HttpClient client, PushStreamContent content, File file);

        Task<IEntry> FolderInfo(RemotePath path, int offset = 0, int limit = int.MaxValue, int depth = 1);

        //Task<FolderInfoResult> ItemInfo(string path, double z, byte k, int offset = 0, int limit = int.MaxValue);
        //Task<FolderInfoResult> ItemInfo(Uri url, int offset = 0, int limit = int.MaxValue);
        Task<FolderInfoResult> ItemInfo(RemotePath path, int offset = 0, int limit = int.MaxValue);

        Task<AccountInfoResult> AccountInfo();



        Task<CreateFolderResult> CreateFolder(string path);

        Task<AddFileResult> AddFile(string fileFullPath, string fileHash, FileSize fileSize, DateTime dateTime, ConflictResolver? conflictResolver);

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


        string PublicBaseUrlDefault { get; }
    }
}
