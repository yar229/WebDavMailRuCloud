using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using YaR.MailRuCloud.Api.Base.Auth;
using YaR.MailRuCloud.Api.Base.Requests;
using YaR.MailRuCloud.Api.Base.Requests.Types;
using YaR.MailRuCloud.Api.Base.Requests.WebV2;
using YaR.MailRuCloud.Api.Base.Streams;
using YaR.MailRuCloud.Api.Common;
using YaR.MailRuCloud.Api.Extensions;
using YaR.MailRuCloud.Api.Links;

namespace YaR.MailRuCloud.Api.Base.Repos
{
    class WebV2RequestRepo: IRequestRepo
    {
        private static readonly log4net.ILog Logger = log4net.LogManager.GetLogger(typeof(WebV2RequestRepo));

        public HttpCommonSettings HttpSettings { get; } = new HttpCommonSettings
        {
            ClientId = String.Empty,
            UserAgent = "Mozilla / 5.0(Windows; U; Windows NT 5.1; en - US; rv: 1.9.0.1) Gecko / 2008070208 Firefox / 3.0.1"
        };

        public int PendingDownloads { get; set; }

        public WebV2RequestRepo(IWebProxy proxy, IAuth auth)
        {
            HttpSettings.Proxy = proxy;

            Authent = auth;

            _bannedShards = new Cached<List<ShardInfo>>(old => new List<ShardInfo>(),
                value => TimeSpan.FromMinutes(2));

            _cachedShards = new Cached<Dictionary<ShardType, ShardInfo>>(old => new ShardInfoRequest(HttpSettings, Authent).MakeRequestAsync().Result.ToShardInfo(),
                value => TimeSpan.FromSeconds(ShardsExpiresInSec));
        }

        public IAuth Authent { get; }



        private readonly Cached<Dictionary<ShardType, ShardInfo>> _cachedShards;
        private readonly Cached<List<ShardInfo>> _bannedShards;
        private const int ShardsExpiresInSec = 30 * 60;


        //TODO: broken now, replace with PUT request
        public HttpWebRequest UploadRequest(ShardInfo shard, File file, UploadMultipartBoundary boundary)
        {
            var url = new Uri($"{shard.Url}?cloud_domain=2&{Authent.Login}");

            var request = (HttpWebRequest)WebRequest.Create(url.OriginalString);
            request.Proxy = HttpSettings.Proxy;
            request.CookieContainer = Authent.Cookies;
            request.Method = "POST";
            request.ContentLength = file.OriginalSize + boundary.Start.LongLength + boundary.End.LongLength;
            request.Referer = $"{ConstSettings.CloudDomain}/home/{Uri.EscapeDataString(file.Path)}";
            request.Headers.Add("Origin", ConstSettings.CloudDomain);
            request.Host = url.Host;
            request.ContentType = $"multipart/form-data; boundary=----{boundary.Guid}";
            request.Accept = "*/*";
            request.UserAgent = HttpSettings.UserAgent;
            request.AllowWriteStreamBuffering = false;
            return request;
        }

        public Stream GetDownloadStream(File file, long? start = null, long? end = null)
        {
            throw new NotImplementedException();
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

        public async Task<IEntry> FolderInfo(string path, Link ulink, int offset = 0, int limit = Int32.MaxValue)
        {

            FolderInfoResult datares;
            try
            {
                datares = await new FolderInfoRequest(HttpSettings, Authent, ulink != null ? ulink.Href : path, ulink != null, offset, limit).MakeRequestAsync();
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

            //var res = req;
            //return res;
        }

        public async Task<FolderInfoResult> ItemInfo(string path, bool isWebLink = false, int offset = 0, int limit = Int32.MaxValue)
        {
            var req = await new ItemInfoRequest(HttpSettings, Authent, path, isWebLink, offset, limit).MakeRequestAsync();
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

        public async Task<UnpublishResult> Unpublish(string publicLink)
        {
            var req = await new UnpublishRequest(HttpSettings, Authent, publicLink).MakeRequestAsync();
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



        public async Task<Dictionary<ShardType, ShardInfo>> ShardInfo()
        {
            var req = await new ShardInfoRequest(HttpSettings, Authent).MakeRequestAsync();
            var res = req.ToShardInfo();
            return res;
        }

        public async Task<CreateFolderResult> CreateFolder(string path)
        {
            return (await new Requests.WebV2.CreateFolderRequest(HttpSettings, Authent, path).MakeRequestAsync())
                .ToCreateFolderResult();
        }

        public async Task<AddFileResult> AddFile(string fileFullPath, string fileHash, FileSize fileSize, DateTime dateTime, ConflictResolver? conflictResolver)
        {
            var res = await new Requests.WebV2.CreateFileRequest(HttpSettings, Authent, fileFullPath, fileHash, fileSize, conflictResolver)
                .MakeRequestAsync();

            return res.ToAddFileResult();
        }


    }
}