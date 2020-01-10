using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using YaR.Clouds.Base.Repos.MailRuCloud;
using YaR.Clouds.Base.Repos.YandexDisk.YadWeb.Models;
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





        public YadWebRequestRepo(IWebProxy proxy, IBasicCredentials creds)
        {
            ServicePointManager.DefaultConnectionLimit = int.MaxValue;

            HttpSettings.Proxy = proxy;
            Authent = new YadWebAuth(HttpSettings, creds);

            CachedSharedList = new Cached<Dictionary<string, IEnumerable<PublicLinkInfo>>>(old =>
                {
                    var res = GetShareListInner().Result;
                    return res;
                }, 
                value => TimeSpan.FromSeconds(30));
        }

        private async Task<Dictionary<string, IEnumerable<PublicLinkInfo>>> GetShareListInner()
        {
            await new YaDCommonRequest(HttpSettings, (YadWebAuth) Authent)
                .With(new YadFolderInfoPostModel("/", "/published"),
                    out YadResponceModel<YadFolderInfoRequestData, YadFolderInfoRequestParams> folderInfo)
                .MakeRequestAsync();

            var res = folderInfo.Data.Resources
                .ToDictionary(
                    it => it.Path.Remove(0, "/disk".Length), 
                    it => Enumerable.Repeat(new PublicLinkInfo("short", it.Meta.UrlShort), 1));

            return res;
        }

        public IAuth Authent { get; }

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
                        out YadResponceModel<ResourceUrlData, ResourceUrlParams> itemInfo)
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
            return null;
        }

        public bool SupportsAddSmallFileByHash => false;

        private HttpRequestMessage CreateUploadClientRequest(PushStreamContent content, File file)
        {

            var _ = new YaDCommonRequest(HttpSettings, (YadWebAuth) Authent)
                .With(new YadGetResourceUploadUrlPostModel(file.FullPath, file.OriginalSize),
                    out YadResponceModel<ResourceUploadUrlData, ResourceUploadUrlParams> itemInfo)
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

            return ures;
        }



        public async Task<IEntry> FolderInfo(RemotePath path, int offset = 0, int limit = Int32.MaxValue, int depth = 1)
        {
            if (path.IsLink)
                throw new NotImplementedException(nameof(FolderInfo));

            await new YaDCommonRequest(HttpSettings, (YadWebAuth) Authent)
                .With(new YadItemInfoPostModel(path.Path),
                    out YadResponceModel<YadItemInfoRequestData, YadItemInfoRequestParams> itemInfo)
                .With(new YadFolderInfoPostModel(path.Path),
                    out YadResponceModel<YadFolderInfoRequestData, YadFolderInfoRequestParams> folderInfo)
                .MakeRequestAsync();

            var itdata = itemInfo?.Data;
            if (itdata?.Type == null)
                return null;

            if (itdata.Type == "file")
                return itdata.ToFile(PublicBaseUrlDefault);

            var entry = folderInfo.Data.ToFolder(itemInfo.Data, path.Path, PublicBaseUrlDefault);

            return entry;
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
                    out YadResponceModel<YadAccountInfoRequestData, YadAccountInfoRequestParams> itemInfo)
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
                    out YadResponceModel<YadCreateFolderRequestData, YadCreateFolderRequestParams> itemInfo)
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
                    out YadResponceModel<YadCopyRequestData, YadCopyRequestParams> itemInfo)
                .MakeRequestAsync();

            var res = itemInfo.ToCopyResult();
            return res;
        }

        public async Task<CopyResult> Move(string sourceFullPath, string destinationPath, ConflictResolver? conflictResolver = null)
        {
            string destFullPath = WebDavPath.Combine(destinationPath, WebDavPath.Name(sourceFullPath));

            //var req = await new YadMoveRequest(HttpSettings, (YadWebAuth)Authent, sourceFullPath, destFullPath)
            //    .MakeRequestAsync();

            await new YaDCommonRequest(HttpSettings, (YadWebAuth) Authent)
                .With(new YadMovePostModel(sourceFullPath, destFullPath), out YadResponceModel<YadMoveRequestData, YadMoveRequestParams> itemInfo)
                .MakeRequestAsync();

            var res = itemInfo.ToMoveResult();
            return res;
        }

        public async Task<PublishResult> Publish(string fullPath)
        {
            await new YaDCommonRequest(HttpSettings, (YadWebAuth) Authent)
                .With(new YadPublishPostModel(fullPath, false), out YadResponceModel<YadPublishRequestData, YadPublishRequestParams> itemInfo)
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
                .With(new YadPublishPostModel(fullPath, true), out YadResponceModel<YadPublishRequestData, YadPublishRequestParams> itemInfo)
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
                    out YadResponceModel<YadDeleteRequestData, YadDeleteRequestParams> itemInfo)
                .MakeRequestAsync();

            var res = itemInfo.ToRemoveResult();
            return res;
        }

        public async Task<RenameResult> Rename(string fullPath, string newName)
        {
            string destPath = WebDavPath.Parent(fullPath);
            destPath = WebDavPath.Combine(destPath, newName);

            //var req = await new YadMoveRequest(HttpSettings, (YadWebAuth)Authent, fullPath, destPath).MakeRequestAsync();

            await new YaDCommonRequest(HttpSettings, (YadWebAuth) Authent)
                .With(new YadMovePostModel(fullPath, destPath),
                    out YadResponceModel<YadMoveRequestData, YadMoveRequestParams> itemInfo)
                .MakeRequestAsync();

            var res = itemInfo.ToRenameResult();
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



        public Cached<Dictionary<string, IEnumerable<PublicLinkInfo>>> CachedSharedList { get; }

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
