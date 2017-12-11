using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mime;
using System.Threading;
using System.Threading.Tasks;
using YaR.MailRuCloud.Api.Base.Requests.Mobile;
using YaR.MailRuCloud.Api.Base.Requests.Types;
using YaR.MailRuCloud.Api.Base.Requests.WebM1;
using YaR.MailRuCloud.Api.Base.Threads;
using YaR.MailRuCloud.Api.Extensions;
using YaR.MailRuCloud.Api.Links;
using AccountInfoRequest = YaR.MailRuCloud.Api.Base.Requests.WebM1.AccountInfoRequest;

namespace YaR.MailRuCloud.Api.Base.Requests.Repo
{
    class WebM1RequestRepo : IRequestRepo
    {
        private readonly IWebProxy _proxy;
        private static readonly log4net.ILog Logger = log4net.LogManager.GetLogger(typeof(WebV2RequestRepo));


        public WebM1RequestRepo(IWebProxy proxy, IAuth auth)
        {
            _proxy = proxy;

            Authent = auth;

            _bannedShards = new Cached<List<ShardInfo>>(() => new List<ShardInfo>(),
                TimeSpan.FromMinutes(2));

            _cachedShards = new Cached<Dictionary<ShardType, ShardInfo>>(() => new ShardInfoRequest(_proxy, Authent).MakeRequestAsync().Result.ToShardInfo(),
                TimeSpan.FromSeconds(ShardsExpiresInSec));

            _metaServer = new Cached<MobMetaServerRequest.Result>(() =>
                {
                    Logger.Debug("MetaServer expired, refreshing.");
                    var server = new MobMetaServerRequest(_proxy).MakeRequestAsync().Result;
                    return server;
                },
                TimeSpan.FromSeconds(MetaServerExpiresSec));

            _downloadServer = new Cached<MobDownloadServerRequest.Result>(() =>
                {
                    Logger.Debug("DownloadServer expired, refreshing.");
                    var server = new MobDownloadServerRequest(_proxy).MakeRequestAsync().Result;
                    return server;
                },
                TimeSpan.FromSeconds(DownloadServerExpiresSec));
        }

        private readonly Cached<MobMetaServerRequest.Result> _metaServer;
        private const int MetaServerExpiresSec = 20 * 60;

        private readonly Cached<MobDownloadServerRequest.Result> _downloadServer;
        private const int DownloadServerExpiresSec = 20 * 60;


        public IAuth Authent { get; }



        private readonly Cached<Dictionary<ShardType, ShardInfo>> _cachedShards;
        private readonly Cached<List<ShardInfo>> _bannedShards;
        private const int ShardsExpiresInSec = 30 * 60;


        public HttpWebRequest UploadRequest(ShardInfo shard, File file, UploadMultipartBoundary boundary)
        {
            var url = new Uri($"{shard.Url}?cloud_domain=2&{Authent.Login}");

            var request = (HttpWebRequest)WebRequest.Create(url.OriginalString);
            request.Proxy = _proxy;
            request.CookieContainer = Authent.Cookies;
            request.Method = "POST";
            request.ContentLength = file.OriginalSize + boundary.Start.LongLength + boundary.End.LongLength;
            request.Referer = $"{ConstSettings.CloudDomain}/home/{Uri.EscapeDataString(file.Path)}";
            request.Headers.Add("Origin", ConstSettings.CloudDomain);
            request.Host = url.Host;
            request.ContentType = $"multipart/form-data; boundary=----{boundary.Guid}";
            request.Accept = "*/*";
            request.UserAgent = ConstSettings.UserAgent;
            request.AllowWriteStreamBuffering = false;
            return request;
        }

        public HttpWebRequest DownloadRequest(long instart, long inend, File file, ShardInfo shard)
        {
            string url = shard.Type == ShardType.Get
                ? $"{_downloadServer.Value.Url}{Uri.EscapeDataString(file.FullPath)}?token={Authent.AccessToken}"
                : $"{shard.Url}{new Uri(ConstSettings.PublishFileLink + file.PublicLink).PathAndQuery.Remove(0, "/public".Length)}?access_token={Authent.AccessToken}";

            var request = (HttpWebRequest)WebRequest.Create(url);

            request.Headers.Add("Accept-Ranges", "bytes");
            request.AddRange(instart, inend);
            request.Proxy = _proxy;
            request.CookieContainer = Authent.Cookies;
            request.Method = "GET";
            request.ContentType = MediaTypeNames.Application.Octet;
            request.Accept = "*/*";
            request.UserAgent = ConstSettings.UserAgent;
            request.AllowReadStreamBuffering = false;

            request.Timeout = 15 * 1000;

            return request;
        }


        public void BanShardInfo(ShardInfo banShard)
        {
            if (!_bannedShards.Value.Any(bsh => bsh.Type == banShard.Type && bsh.Url == banShard.Url))
            {
                Logger.Warn($"Shard {banShard.Url} temporarily banned");
                _bannedShards.Value.Add(banShard);
            }
        }


        /// <summary>
        /// Get shard info that to do post get request. Can be use for anonymous user.
        /// </summary>
        /// <param name="shardType">Shard type as numeric type.</param>
        /// <returns>Shard info.</returns>
        public async Task<ShardInfo> GetShardInfo(ShardType shardType)
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
            var req = await new CloneItemRequest(_proxy, Authent, fromUrl, toPath).MakeRequestAsync();
            var res = req.ToCloneItemResult();
            return res;
        }

        public async Task<CopyResult> Copy(string sourceFullPath, string destinationPath, ConflictResolver? conflictResolver = null)
        {
            var req = await new CopyRequest(_proxy, Authent, sourceFullPath, destinationPath, conflictResolver).MakeRequestAsync();
            var res = req.ToCopyResult();
            return res;
        }

        public async Task<CopyResult> Move(string sourceFullPath, string destinationPath, ConflictResolver? conflictResolver = null)
        {
            var req = await new MoveRequest(_proxy, Authent, sourceFullPath, destinationPath).MakeRequestAsync();
            var res = req.ToCopyResult();
            return res;
        }

        public async Task<IEntry> FolderInfo(string path, Link ulink, int offset = 0, int limit = Int32.MaxValue)
        {

            FolderInfoResult datares;
            try
            {
                datares = await new FolderInfoRequest(_proxy, Authent, ulink != null ? ulink.Href : path, ulink != null, offset, limit).MakeRequestAsync();
            }
            catch (WebException e) when ((e.Response as HttpWebResponse)?.StatusCode == HttpStatusCode.NotFound)
            {
                return null;
            }

            MailRuCloud.ItemType itemType;
            if (null == ulink)
                itemType = datares.body.home == path
                    ? MailRuCloud.ItemType.Folder
                    : MailRuCloud.ItemType.File;
            else
                itemType = ulink.ItemType;


            var entry = itemType == MailRuCloud.ItemType.File
                ? (IEntry)datares.ToFile(
                    home: WebDavPath.Parent(path),
                    ulink: ulink,
                    filename: ulink == null ? WebDavPath.Name(path) : ulink.OriginalName,
                    nameReplacement: WebDavPath.Name(path))
                : datares.ToFolder(path, ulink);

            return entry;
        }

        public async Task<FolderInfoResult> ItemInfo(string path, bool isWebLink = false, int offset = 0, int limit = Int32.MaxValue)
        {
            var req = await new ItemInfoRequest(_proxy, Authent, path, isWebLink, offset, limit).MakeRequestAsync();
            var res = req;
            return res;
        }

        public async Task<AccountInfoResult> AccountInfo()
        {
            var req = await new AccountInfoRequest(_proxy, Authent).MakeRequestAsync();
            var res = req.ToAccountInfo();
            return res;
        }

        public async Task<PublishResult> Publish(string fullPath)
        {
            var req = await new PublishRequest(_proxy, Authent, fullPath).MakeRequestAsync();
            var res = req.ToPublishResult();
            return res;
        }

        public async Task<UnpublishResult> Unpublish(string publicLink)
        {
            var req = await new UnpublishRequest(_proxy, Authent, publicLink).MakeRequestAsync();
            var res = req.ToUnpublishResult();
            return res;
        }

        public async Task<RemoveResult> Remove(string fullPath)
        {
            var req = await new RemoveRequest(_proxy, Authent, fullPath).MakeRequestAsync();
            var res = req.ToRemoveResult();
            return res;
        }

        public async Task<RenameResult> Rename(string fullPath, string newName)
        {
            string target = WebDavPath.Combine(WebDavPath.Parent(fullPath), newName);

            await new Mobile.RenameRequest(_proxy, Authent, _metaServer.Value.Url, fullPath, target)
                .MakeRequestAsync();
            var res = new RenameResult {IsSuccess = true};
            return res;
        }



        public async Task<Dictionary<ShardType, ShardInfo>> ShardInfo()
        {
            var req = await new ShardInfoRequest(_proxy, Authent).MakeRequestAsync();
            var res = req.ToShardInfo();
            return res;
        }

        public async Task<CreateFolderResult> CreateFolder(string path)
        {
            return (await new WebV2.CreateFolderRequest(_proxy, Authent, path).MakeRequestAsync())
                .ToCreateFolderResult();
        }

        public async Task<AddFileResult> AddFile(string fileFullPath, string fileHash, FileSize fileSize, DateTime dateTime, ConflictResolver? conflictResolver)
        {
            var res = await new WebV2.CreateFileRequest(_proxy, Authent, fileFullPath, fileHash, fileSize, conflictResolver)
                .MakeRequestAsync();

            return res.ToAddFileResult();
        }


    }
}
