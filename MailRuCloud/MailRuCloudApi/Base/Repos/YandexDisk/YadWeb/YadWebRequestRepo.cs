using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using YaR.Clouds.Base.Repos.MailRuCloud;
using YaR.Clouds.Base.Repos.YandexDisk.YadWeb.Models;
using YaR.Clouds.Base.Repos.YandexDisk.YadWeb.Models.Media;
using YaR.Clouds.Base.Repos.YandexDisk.YadWeb.Requests;
using YaR.Clouds.Base.Requests;
using YaR.Clouds.Base.Requests.Types;
using YaR.Clouds.Base.Streams;
using YaR.Clouds.Common;
using Stream = System.IO.Stream;

namespace YaR.Clouds.Base.Repos.YandexDisk.YadWeb
{
    class YadWebRequestRepo : IRequestRepo
    {
        private static readonly log4net.ILog Logger = log4net.LogManager.GetLogger(typeof(YadWebRequestRepo));

        private ItemOperation _lastRemoveOperation;
        
        private const int OperationStatusCheckIntervalMs = 300;
        private const int OperationStatusCheckRetryCount = 8;

        private readonly IBasicCredentials _creds;

        public YadWebRequestRepo(IWebProxy proxy, IBasicCredentials creds)
        {
            ServicePointManager.DefaultConnectionLimit = int.MaxValue;

            HttpSettings.Proxy = proxy;
            _creds = creds;
        }

        private async Task<Dictionary<string, IEnumerable<PublicLinkInfo>>> GetShareListInner()
        {
            await new YaDCommonRequest(HttpSettings, (YadWebAuth) Authent)
                .With(new YadFolderInfoPostModel("/", "/published"),
                    out YadResponseModel<YadFolderInfoRequestData, YadFolderInfoRequestParams> folderInfo)
                .MakeRequestAsync();

            var res = folderInfo.Data.Resources
                .Where(it => !string.IsNullOrEmpty(it.Meta?.UrlShort))
                .ToDictionary(
                    it => it.Path.Remove(0, "/disk".Length), 
                    it => Enumerable.Repeat(new PublicLinkInfo("short", it.Meta.UrlShort), 1));

            return res;
        }

        public IAuth Authent => CachedAuth.Value;

        private Cached<YadWebAuth> CachedAuth => _cachedAuth ??= new Cached<YadWebAuth>(auth => new YadWebAuth(HttpSettings, _creds), auth => TimeSpan.FromHours(23));
        private Cached<YadWebAuth> _cachedAuth;

        public Cached<Dictionary<string, IEnumerable<PublicLinkInfo>>> CachedSharedList => _cachedSharedList ??= new Cached<Dictionary<string, IEnumerable<PublicLinkInfo>>>(old =>
                    {
                        var res = GetShareListInner().Result;
                        return res;
                    }, 
                    value => TimeSpan.FromSeconds(30));
        private Cached<Dictionary<string, IEnumerable<PublicLinkInfo>>> _cachedSharedList;


        public HttpCommonSettings HttpSettings { get; } = new HttpCommonSettings
        {
            UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/78.0.3904.108 Safari/537.36"
        };

        public Stream GetDownloadStream(File afile, long? start = null, long? end = null)
        {
            CustomDisposable<HttpWebResponse> ResponseGenerator(long instart, long inend, File file)
            {
                //var urldata = new YadGetResourceUrlRequest(HttpSettings, (YadWebAuth)Authent, file.FullPath)
                //    .MakeRequestAsync()
                //    .Result;

                var _ = new YaDCommonRequest(HttpSettings, (YadWebAuth) Authent)
                    .With(new YadGetResourceUrlPostModel(file.FullPath),
                        out YadResponseModel<ResourceUrlData, ResourceUrlParams> itemInfo)
                    .MakeRequestAsync().Result;

                var url = "https:" + itemInfo.Data.File;
                HttpWebRequest request = new YadDownloadRequest(HttpSettings, (YadWebAuth)Authent, url, instart, inend);
                var response = (HttpWebResponse)request.GetResponse();

                return new CustomDisposable<HttpWebResponse>
                {
                    Value = response,
                    OnDispose = () => {}
                };
            }

            var stream = new DownloadStream(ResponseGenerator, afile, start, end);
            return stream;
        }

        //public HttpWebRequest UploadRequest(File file, UploadMultipartBoundary boundary)
        //{
        //    var urldata = 
        //        new YadGetResourceUploadUrlRequest(HttpSettings, (YadWebAuth)Authent, file.FullPath, file.OriginalSize)
        //        .MakeRequestAsync()
        //        .Result;
        //    var url = urldata.Models[0].Data.UploadUrl;

        //    var result = new YadUploadRequest(HttpSettings, (YadWebAuth)Authent, url, file.OriginalSize);
        //    return result;
        //}

        public ICloudHasher GetHasher()
        {
            return new YadHasher();
        }

        public bool SupportsAddSmallFileByHash => false;

        private HttpRequestMessage CreateUploadClientRequest(PushStreamContent content, File file)
        {

            var _ = new YaDCommonRequest(HttpSettings, (YadWebAuth) Authent)
                .With(new YadGetResourceUploadUrlPostModel(file.FullPath, file.OriginalSize),
                    out YadResponseModel<ResourceUploadUrlData, ResourceUploadUrlParams> itemInfo)
                .MakeRequestAsync().Result;
            var url = itemInfo.Data.UploadUrl;

            var request = new HttpRequestMessage
            {
                RequestUri = new Uri(url),
                Method = HttpMethod.Put
            };

            request.Headers.Add("Accept", "*/*");
            request.Headers.TryAddWithoutValidation("User-Agent", HttpSettings.UserAgent);

            request.Content = content;
            request.Content.Headers.ContentLength = file.OriginalSize;


            return request;
        }

        public async Task<UploadFileResult> DoUpload(HttpClient client, PushStreamContent content, File file)
        {
            var request = CreateUploadClientRequest(content, file);
            var responseMessage = await client.SendAsync(request);
            var ures = responseMessage.ToUploadPathResult();

            //Thread.Sleep(1_000);

            return ures;
        }

        private const string YadMediaPath = "/Media.wdyad";

        public async Task<IEntry> FolderInfo(RemotePath path, int offset = 0, int limit = Int32.MaxValue, int depth = 1)
        {
            if (path.IsLink)
                throw new NotImplementedException(nameof(FolderInfo));

            if (path.Path.StartsWith(YadMediaPath))
                return await MediaFolderInfo(path.Path);

            // YaD perform async deletion
            YadResponseModel<YadItemInfoRequestData, YadItemInfoRequestParams> itemInfo = null;
            YadResponseModel<YadFolderInfoRequestData, YadFolderInfoRequestParams> folderInfo = null;
            YadResponseModel<YadResourceStatsRequestData, YadResourceStatsRequestParams> resourceStats = null;

            bool hasRemoveOp = _lastRemoveOperation != null &&
                               WebDavPath.IsParentOrSame(path.Path, _lastRemoveOperation.Path) &&
                               (DateTime.Now - _lastRemoveOperation.DateTime).TotalMilliseconds < 1_000;
            Retry.Do(
                () =>
                {
                    var doPreSleep = hasRemoveOp ? TimeSpan.FromMilliseconds(OperationStatusCheckIntervalMs) : TimeSpan.Zero;
                    if (doPreSleep > TimeSpan.Zero)
                        Logger.Debug("Has remove op, sleep before");
                    return doPreSleep;
                },
                () => new YaDCommonRequest(HttpSettings, (YadWebAuth) Authent)
                    .With(new YadItemInfoPostModel(path.Path), out itemInfo)
                    .With(new YadFolderInfoPostModel(path.Path), out folderInfo)
                    .With(new YadResourceStatsPostModel(path.Path), out resourceStats)
                    .MakeRequestAsync()
                    .Result,
                resp =>
                {
                    var doAgain = hasRemoveOp &&
                           folderInfo.Data.Resources.Any(r =>
                               WebDavPath.PathEquals(r.Path.Remove(0, "/disk".Length), _lastRemoveOperation.Path));
                    if (doAgain)
                        Logger.Debug("Remove op still not finished, let's try again");
                    return doAgain;
                }, 
                TimeSpan.FromMilliseconds(OperationStatusCheckIntervalMs), OperationStatusCheckRetryCount);
                

            var itdata = itemInfo?.Data;
            if (itdata?.Type == null)
                return null;

            if (itdata.Type == "file")
                return itdata.ToFile(PublicBaseUrlDefault);

            var entry = folderInfo.Data.ToFolder(itemInfo.Data, resourceStats.Data, path.Path, PublicBaseUrlDefault);

            return entry;
        }


        private async Task<IEntry> MediaFolderInfo(string path)
        {
            var root = await MediaFolderRootInfo() as Folder;
            if (null == root)
                return null;
            
            if (WebDavPath.PathEquals(path, YadMediaPath))
                return root;

            string albumName = WebDavPath.Name(path);
            var album = root.Folders.FirstOrDefault(f => f.Name == albumName);
            if (null == album)
                return null;

            _ = new YaDCommonRequest(HttpSettings, (YadWebAuth) Authent)
                .With(new YadFolderInfoPostModel(album.PublicLinks.First().Key, "/album"),
                    out YadResponseModel<YadFolderInfoRequestData, YadFolderInfoRequestParams> folderInfo)
                .MakeRequestAsync()
                .Result;

            var entry = folderInfo.Data.ToFolder(null, null, path, PublicBaseUrlDefault);

            return entry;
        }

        private async Task<IEntry> MediaFolderRootInfo()
        {
            var res = new Folder(YadMediaPath);

            _ = await new YaDCommonRequest(HttpSettings, (YadWebAuth)Authent)
                .With(new YadGetAlbumsSlicesPostModel(),
                    out YadResponseModel<YadGetAlbumsSlicesRequestData, YadGetAlbumsSlicesRequestParams> slices)
                .With(new YadAlbumsPostModel(),
                    out YadResponseModel<YadAlbumsRequestData[], YadAlbumsRequestParams> albums)
                .MakeRequestAsync();

            if (slices.Data.Albums.Camera != null)
                res.Folders.Add(new Folder($"{YadMediaPath}/.{slices.Data.Albums.Camera.Id}")
                { ServerFilesCount = (int)slices.Data.Albums.Camera.Count });
            if (slices.Data.Albums.Photounlim != null)
                res.Folders.Add(new Folder($"{YadMediaPath}/.{slices.Data.Albums.Photounlim.Id}")
                { ServerFilesCount = (int)slices.Data.Albums.Photounlim.Count });
            if (slices.Data.Albums.Videos != null)
                res.Folders.Add(new Folder($"{YadMediaPath}/.{slices.Data.Albums.Videos.Id}")
                { ServerFilesCount = (int)slices.Data.Albums.Videos.Count });

            res.Folders.AddRange(albums.Data.Select(al => new Folder($"{YadMediaPath}/{al.Title}")
            {
                PublicLinks = { new PublicLinkInfo(al.Public.PublicUrl) {Key = al.Public.PublicKey} }
            }));

            return res;
        }


        public Task<FolderInfoResult> ItemInfo(RemotePath path, int offset = 0, int limit = Int32.MaxValue)
        {
            throw new NotImplementedException();
        }


        public async Task<AccountInfoResult> AccountInfo()
        {
            //var req = await new YadAccountInfoRequest(HttpSettings, (YadWebAuth)Authent).MakeRequestAsync();

            await new YaDCommonRequest(HttpSettings, (YadWebAuth) Authent)
                .With(new YadAccountInfoPostModel(),
                    out YadResponseModel<YadAccountInfoRequestData, YadAccountInfoRequestParams> itemInfo)
                .MakeRequestAsync();

            var res = itemInfo.ToAccountInfo();
            return res;
        }

        public async Task<CreateFolderResult> CreateFolder(string path)
        {
            //var req = await new YadCreateFolderRequest(HttpSettings, (YadWebAuth)Authent, path)
            //    .MakeRequestAsync();

            await new YaDCommonRequest(HttpSettings, (YadWebAuth) Authent)
                .With(new YadCreateFolderPostModel(path),
                    out YadResponseModel<YadCreateFolderRequestData, YadCreateFolderRequestParams> itemInfo)
                .MakeRequestAsync();

            var res = itemInfo.Params.ToCreateFolderResult();
            return res;
        }

        public async Task<AddFileResult> AddFile(string fileFullPath, string fileHash, FileSize fileSize, DateTime dateTime,
            ConflictResolver? conflictResolver)
        {
            var res = new AddFileResult
            {
                Path = fileFullPath,
                Success = true
            };

            return await Task.FromResult(res);
        }

        public Task<CloneItemResult> CloneItem(string fromUrl, string toPath)
        {
            throw new NotImplementedException();
        }

        public async Task<CopyResult> Copy(string sourceFullPath, string destinationPath, ConflictResolver? conflictResolver = null)
        {
            string destFullPath = WebDavPath.Combine(destinationPath, WebDavPath.Name(sourceFullPath));

            //var req = await new YadCopyRequest(HttpSettings, (YadWebAuth)Authent, sourceFullPath, destFullPath)
            //    .MakeRequestAsync();

            await new YaDCommonRequest(HttpSettings, (YadWebAuth) Authent)
                .With(new YadCopyPostModel(sourceFullPath, destFullPath),
                    out YadResponseModel<YadCopyRequestData, YadCopyRequestParams> itemInfo)
                .MakeRequestAsync();

            var res = itemInfo.ToCopyResult();
            return res;
        }

        public async Task<CopyResult> Move(string sourceFullPath, string destinationPath, ConflictResolver? conflictResolver = null)
        {
            string destFullPath = WebDavPath.Combine(destinationPath, WebDavPath.Name(sourceFullPath));

            await new YaDCommonRequest(HttpSettings, (YadWebAuth) Authent)
                .With(new YadMovePostModel(sourceFullPath, destFullPath), out YadResponseModel<YadMoveRequestData, YadMoveRequestParams> itemInfo)
                .MakeRequestAsync();

            var res = itemInfo.ToMoveResult();

            WaitForOperation(itemInfo.Data.Oid);

            return res;
        }


        private void WaitForOperation(string operationOid)
        {
            YadResponseModel<YadOperationStatusData, YadOperationStatusParams> itemInfo = null;
            Retry.Do(
                () => TimeSpan.Zero,
                () => new YaDCommonRequest(HttpSettings, (YadWebAuth) Authent)
                    .With(new YadOperationStatusPostModel(operationOid), out itemInfo)
                    .MakeRequestAsync()
                    .Result,
                resp =>
                {
                    var doAgain = null == itemInfo.Data.Error && itemInfo.Data.State != "COMPLETED";
                    //if (doAgain)
                    //    Logger.Debug("Move op still not finished, let's try again");
                    return doAgain;
                }, 
                TimeSpan.FromMilliseconds(OperationStatusCheckIntervalMs), OperationStatusCheckRetryCount);
        }

        public async Task<PublishResult> Publish(string fullPath)
        {
            await new YaDCommonRequest(HttpSettings, (YadWebAuth) Authent)
                .With(new YadPublishPostModel(fullPath, false), out YadResponseModel<YadPublishRequestData, YadPublishRequestParams> itemInfo)
                .MakeRequestAsync();

            var res = itemInfo.ToPublishResult();

            if (res.IsSuccess)
                CachedSharedList.Value[fullPath] = new List<PublicLinkInfo> {new PublicLinkInfo(res.Url)};

            return res;
        }

        public async Task<UnpublishResult> Unpublish(Uri publicLink, string fullPath)
        {
            foreach (var item in CachedSharedList.Value
                .Where(kvp => kvp.Key == fullPath).ToList())
            {
                CachedSharedList.Value.Remove(item.Key);
            }

            await new YaDCommonRequest(HttpSettings, (YadWebAuth) Authent)
                .With(new YadPublishPostModel(fullPath, true), out YadResponseModel<YadPublishRequestData, YadPublishRequestParams> itemInfo)
                .MakeRequestAsync();

            var res = itemInfo.ToUnpublishResult();

            return res;
        }

        public async Task<RemoveResult> Remove(string fullPath)
        {
            //var req = await new YadDeleteRequest(HttpSettings, (YadWebAuth)Authent, fullPath)
            //    .MakeRequestAsync();

            await new YaDCommonRequest(HttpSettings, (YadWebAuth) Authent)
                .With(new YadDeletePostModel(fullPath),
                    out YadResponseModel<YadDeleteRequestData, YadDeleteRequestParams> itemInfo)
                .MakeRequestAsync();

            var res = itemInfo.ToRemoveResult();
                
            if (res.IsSuccess)
                _lastRemoveOperation = res.ToItemOperation();

            return res;
        }

        public async Task<RenameResult> Rename(string fullPath, string newName)
        {
            string destPath = WebDavPath.Parent(fullPath);
            destPath = WebDavPath.Combine(destPath, newName);

            //var req = await new YadMoveRequest(HttpSettings, (YadWebAuth)Authent, fullPath, destPath).MakeRequestAsync();

            await new YaDCommonRequest(HttpSettings, (YadWebAuth) Authent)
                .With(new YadMovePostModel(fullPath, destPath),
                    out YadResponseModel<YadMoveRequestData, YadMoveRequestParams> itemInfo)
                .MakeRequestAsync();

            var res = itemInfo.ToRenameResult();

            if (res.IsSuccess)
                WaitForOperation(itemInfo.Data.Oid);

            //if (res.IsSuccess)
            //    _lastRemoveOperation = res.ToItemOperation();


            return res;
        }

        public Dictionary<ShardType, ShardInfo> GetShardInfo1()
        {
            throw new NotImplementedException();
        }


        public IEnumerable<PublicLinkInfo> GetShareLinks(string path)
        {
            if (CachedSharedList.Value.TryGetValue(path, out var links))
                foreach (var link in links)
                    yield return link;
        }

        
        public async void CleanTrash()
        {
            await new YaDCommonRequest(HttpSettings, (YadWebAuth) Authent)
                .With(new YadCleanTrashPostModel(), 
                    out YadResponseModel<YadCleanTrashData, YadCleanTrashParams> _)
                .MakeRequestAsync();
        }


        

        public IEnumerable<string> PublicBaseUrls { get; set; } = new[]
        {
            "https://yadi.sk"
        };
        public string PublicBaseUrlDefault => PublicBaseUrls.First();







        public string ConvertToVideoLink(Uri publicLink, SharedVideoResolution videoResolution)
        {
            throw new NotImplementedException("Yad not implemented ConvertToVideoLink");
        }
    }

    //public static class Zzz
    //{
    //    private static Stopwatch _sw = new Stopwatch();

    //    static Zzz()
    //    {
    //        _sw.Start();
    //    }

    //    public static long ElapsedMs()
    //    {
    //        return _sw.ElapsedMilliseconds;
    //    }
    //}
}
