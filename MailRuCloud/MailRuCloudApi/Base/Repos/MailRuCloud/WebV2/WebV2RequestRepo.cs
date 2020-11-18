using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using YaR.Clouds.Base.Repos.MailRuCloud.WebV2.Requests;
using YaR.Clouds.Base.Requests;
using YaR.Clouds.Base.Requests.Types;
using YaR.Clouds.Base.Streams;
using YaR.Clouds.Common;

namespace YaR.Clouds.Base.Repos.MailRuCloud.WebV2
{
    class WebV2RequestRepo: MailRuBaseRepo, IRequestRepo
    {
        private static readonly log4net.ILog Logger = log4net.LogManager.GetLogger(typeof(WebV2RequestRepo));

        public sealed override HttpCommonSettings HttpSettings { get; } = new HttpCommonSettings
        {
            ClientId = String.Empty,
            UserAgent = "Mozilla / 5.0(Windows; U; Windows NT 5.1; en - US; rv: 1.9.0.1) Gecko / 2008070208 Firefox / 3.0.1"
        };

        public WebV2RequestRepo(IWebProxy proxy, IBasicCredentials creds, AuthCodeRequiredDelegate onAuthCodeRequired)
            :base(creds)
        {
            HttpSettings.Proxy = proxy;

            Authent = new WebAuth(HttpSettings, creds, onAuthCodeRequired);

            _bannedShards = new Cached<List<ShardInfo>>(old => new List<ShardInfo>(),
                value => TimeSpan.FromMinutes(2));

            _cachedShards = new Cached<Dictionary<ShardType, ShardInfo>>(old => new ShardInfoRequest(HttpSettings, Authent).MakeRequestAsync().Result.ToShardInfo(),
                value => TimeSpan.FromSeconds(ShardsExpiresInSec));
        }

        



        private readonly Cached<Dictionary<ShardType, ShardInfo>> _cachedShards;
        private readonly Cached<List<ShardInfo>> _bannedShards;
        private const int ShardsExpiresInSec = 30 * 60;


        //public HttpWebRequest UploadRequest(File file, UploadMultipartBoundary boundary)
        //{
        //    var shard = GetShardInfo(ShardType.Upload).Result;

        //    var url = new Uri($"{shard.Url}?cloud_domain=2&{Authent.Login}");

        //    var result = new UploadRequest(url.OriginalString, file, Authent, HttpSettings);
        //    return result;
        //}

        public Stream GetDownloadStream(File afile, long? start = null, long? end = null)
        {

            CustomDisposable<HttpWebResponse> ResponseGenerator(long instart, long inend, File file)
            {
                HttpWebRequest request = new DownloadRequest(file, instart, inend, Authent, HttpSettings, _cachedShards);
                var response = (HttpWebResponse)request.GetResponse();

                return new CustomDisposable<HttpWebResponse>
                {
                    Value = response,
                    OnDispose = () =>
                    {
                        //_shardManager.DownloadServersPending.Free(downServer);
                        //watch.Stop();
                        //Logger.Debug($"HTTP:{request.Method}:{request.RequestUri.AbsoluteUri} ({watch.Elapsed.Milliseconds} ms)");
                    }
                };
            }

            var stream = new DownloadStream(ResponseGenerator, afile, start, end);
            return stream;
        }

        //public HttpWebRequest DownloadRequest(long instart, long inend, File file, ShardInfo shard)
        //{
        //    string downloadkey = string.Empty;
        //    if (shard.Type == ShardType.WeblinkGet)
        //        downloadkey = Authent.DownloadToken;

        //    string url = shard.Type == ShardType.Get
        //        ? $"{shard.Url}{Uri.EscapeDataString(file.FullPath)}"
        //        : $"{shard.Url}{new Uri(ConstSettings.PublishFileLink + file.PublicLink).PathAndQuery.Remove(0, "/public".Length)}?key={downloadkey}";

        //    var request = (HttpWebRequest)WebRequest.Create(url);

        //    request.Headers.Add("Accept-Ranges", "bytes");
        //    request.AddRange(instart, inend);
        //    request.Proxy = HttpSettings.Proxy;
        //    request.CookieContainer = Authent.Cookies;
        //    request.Method = "GET";
        //    request.ContentType = MediaTypeNames.Application.Octet;
        //    request.Accept = "*/*";
        //    request.UserAgent = HttpSettings.UserAgent;
        //    request.AllowReadStreamBuffering = false;

        //    request.Timeout = 15 * 1000;

        //    return request;
        //}


        //public void BanShardInfo(ShardInfo banShard)
        //{
        //    if (!_bannedShards.Value.Any(bsh => bsh.Type == banShard.Type && bsh.Url == banShard.Url))
        //    {
        //        Logger.Warn($"Shard {banShard.Url} temporarily banned");
        //        _bannedShards.Value.Add(banShard);
        //    }
        //}


        /// <summary>
        /// Get shard info that to do post get request. Can be use for anonymous user.
        /// </summary>
        /// <param name="shardType">Shard type as numeric type.</param>
        /// <returns>Shard info.</returns>
        public override async Task<ShardInfo> GetShardInfo(ShardType shardType)
        {
            bool refreshed = false;
            for (int i = 0; i < 10; i++)
            {
                Thread.Sleep(80 * i);
                var ishards = await Task.Run(() => _cachedShards.Value);
                var ishard = ishards[shardType];
                var banned = _bannedShards.Value;
                if (banned.All(bsh => bsh.Url != ishard.Url))
                {
                    if (refreshed) Authent.ExpireDownloadToken();
                    return ishard;
                }
                _cachedShards.Expire();
                refreshed = true;
            }

            Logger.Error("Cannot get working shard.");

            var shards = await Task.Run(() => _cachedShards.Value);
            var shard = shards[shardType];
            return shard;
        }

        public async Task<CloneItemResult> CloneItem(string fromUrl, string toPath)
        {
            var req = await new CloneItemRequest(HttpSettings, Authent, fromUrl, toPath).MakeRequestAsync();
            var res = req.ToCloneItemResult();
            return res;
        }

        public async Task<CopyResult> Copy(string sourceFullPath, string destinationPath, ConflictResolver? conflictResolver = null)
        {
            var req = await new CopyRequest(HttpSettings, Authent, sourceFullPath, destinationPath, conflictResolver).MakeRequestAsync();
            var res = req.ToCopyResult();
            return res;
        }

        public async Task<CopyResult> Move(string sourceFullPath, string destinationPath, ConflictResolver? conflictResolver = null)
        {
            var req = await new MoveRequest(HttpSettings, Authent, sourceFullPath, destinationPath).MakeRequestAsync();
            var res = req.ToCopyResult();
            return res;
        }

	    /// <summary>
		/// 
		/// </summary>
		/// <param name="path"></param>
		/// <param name="offset"></param>
		/// <param name="limit"></param>
		/// <param name="depth">Not applicable here, always = 1</param>
		/// <returns></returns>
        public async Task<IEntry> FolderInfo(RemotePath path, int offset = 0, int limit = Int32.MaxValue, int depth = 1)
        {

            FolderInfoResult datares;
            try
            {
                datares = await new FolderInfoRequest(HttpSettings, Authent, path, offset, limit).MakeRequestAsync();
            }
            catch (WebException e) when ((e.Response as HttpWebResponse)?.StatusCode == HttpStatusCode.NotFound)
            {
                return null;
            }

            Cloud.ItemType itemType;
            if (null == path.Link || path.Link.ItemType == Cloud.ItemType.Unknown)
                itemType = datares.Body.Home == path.Path ||
                           WebDavPath.PathEquals("/" + datares.Body.Weblink, path.Path)
                           //datares.body.list.Any(fi => "/" + fi.weblink == path)
                    ? Cloud.ItemType.Folder
                    : Cloud.ItemType.File;
            else
                itemType = path.Link.ItemType;


            var entry = itemType == Cloud.ItemType.File
                ? (IEntry)datares.ToFile(
                    PublicBaseUrlDefault,
                    home: WebDavPath.Parent(path.Path ?? string.Empty),
                    ulink: path.Link,
                    filename: path.Link == null ? WebDavPath.Name(path.Path) : path.Link.OriginalName,
                    nameReplacement: path.Link?.IsLinkedToFileSystem ?? true ? WebDavPath.Name(path.Path) : null)
                : datares.ToFolder(PublicBaseUrlDefault, path.Path, path.Link);

            return entry;
        }

        public async Task<FolderInfoResult> ItemInfo(RemotePath path, int offset = 0, int limit = Int32.MaxValue)
        {
            var req = await new ItemInfoRequest(HttpSettings, Authent, path, offset, limit).MakeRequestAsync();
            var res = req;
            return res;
        }


        public async Task<AccountInfoResult> AccountInfo()
        {
            var req = await new AccountInfoRequest(HttpSettings, Authent).MakeRequestAsync();
            var res = req.ToAccountInfo();
            return res;
        }

        public async Task<PublishResult> Publish(string fullPath)
        {
            var req = await new PublishRequest(HttpSettings, Authent, fullPath).MakeRequestAsync();
            var res = req.ToPublishResult();
            return res;
        }

        public async Task<UnpublishResult> Unpublish(Uri publicLink, string fullPath = null)
        {
            var req = await new UnpublishRequest(HttpSettings, Authent, publicLink.OriginalString).MakeRequestAsync();
            var res = req.ToUnpublishResult();
            return res;
        }

        public async Task<RemoveResult> Remove(string fullPath)
        {
            var req = await new RemoveRequest(HttpSettings, Authent, fullPath).MakeRequestAsync();
            var res = req.ToRemoveResult();
            return res;
        }

        public async Task<RenameResult> Rename(string fullPath, string newName)
        {
            var req = await new RenameRequest(HttpSettings, Authent, fullPath, newName).MakeRequestAsync();
            var res = req.ToRenameResult();
            return res;
        }

        public Dictionary<ShardType, ShardInfo> GetShardInfo1()
        {
            return new ShardInfoRequest(HttpSettings, Authent).MakeRequestAsync().Result.ToShardInfo();
        }

        public IEnumerable<PublicLinkInfo> GetShareLinks(string fullPath)
        {
            throw new NotImplementedException("WebV2 GetShareLink not implemented");
        }

        public void CleanTrash()
        {
            throw new NotImplementedException();
        }

        public async Task<CreateFolderResult> CreateFolder(string path)
        {
            return (await new CreateFolderRequest(HttpSettings, Authent, path).MakeRequestAsync())
                .ToCreateFolderResult();
        }

        public async Task<AddFileResult> AddFile(string fileFullPath, IFileHash fileHash, FileSize fileSize, DateTime dateTime, ConflictResolver? conflictResolver)
        {
            var hash = fileHash.Hash.Value;

            var res = await new CreateFileRequest(HttpSettings, Authent, fileFullPath, hash, fileSize, conflictResolver)
                .MakeRequestAsync();

            return res.ToAddFileResult();
        }
    }
}