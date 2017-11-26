using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using YaR.MailRuCloud.Api.Base.Requests.Types;
using YaR.MailRuCloud.Api.Base.Requests.Web;
using YaR.MailRuCloud.Api.Extensions;

namespace YaR.MailRuCloud.Api.Base.Requests.Repo
{
    class WebRequestRepo: IRequestRepo
    {
        private static readonly log4net.ILog Logger = log4net.LogManager.GetLogger(typeof(WebRequestRepo));
        private readonly CloudApi _cloudApi;

        public WebRequestRepo(CloudApi cloudApi)
        {
            _cloudApi = cloudApi;

            _cachedDownloadToken = new Cached<string>(() => new DownloadTokenRequest(_cloudApi, AuthToken.Value.Token).MakeRequestAsync().Result.ToToken(),
                TimeSpan.FromSeconds(DownloadTokenExpiresSec));

            AuthToken = new Cached<AuthTokenResult>(() =>
                {
                    Logger.Debug("AuthToken expired, refreshing.");
                    var token = Auth().Result;
                    _cachedDownloadToken.Expire();
                    return token;
                },
                TimeSpan.FromSeconds(AuthTokenExpiresInSec));


            _bannedShards = new Cached<List<ShardInfo>>(() => new List<ShardInfo>(),
                TimeSpan.FromMinutes(2));

            _cachedShards = new Cached<Dictionary<ShardType, ShardInfo>>(() => new ShardInfoRequest(_cloudApi, AuthToken.Value.Token).MakeRequestAsync().Result.ToShardInfo(),
                TimeSpan.FromSeconds(ShardsExpiresInSec));
        }

        public async  Task<bool> Login(Account.AuthCodeRequiredDelegate onAuthCodeRequired)
        {
            var loginResult = await new LoginRequest(_cloudApi, _cloudApi.Account.Credentials)
                .MakeRequestAsync();

            // 2FA
            if (!string.IsNullOrEmpty(loginResult.Csrf))
            {
                string authCode = onAuthCodeRequired(_cloudApi.Account.Credentials.Login, false);
                await new SecondStepAuthRequest(_cloudApi, loginResult.Csrf, _cloudApi.Account.Credentials.Login, authCode)
                    .MakeRequestAsync();
            }

            await new EnsureSdcCookieRequest(_cloudApi)
                .MakeRequestAsync();

            return true;
        }



        /// <summary>
        /// Token for authorization
        /// </summary>
        public readonly Cached<AuthTokenResult> AuthToken;
        private const int AuthTokenExpiresInSec = 23 * 60 * 60;

        /// <summary>
        /// Token for downloading files
        /// </summary>
        private readonly Cached<string> _cachedDownloadToken;
        private const int DownloadTokenExpiresSec = 20 * 60;

        private readonly Cached<Dictionary<ShardType, ShardInfo>> _cachedShards;
        private readonly Cached<List<ShardInfo>> _bannedShards;
        private const int ShardsExpiresInSec = 30 * 60;


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
                    if (refreshed) _cachedDownloadToken.Expire();
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



        public async Task<AuthTokenResult> Auth()
        {
            var req = await new AuthTokenRequest(_cloudApi).MakeRequestAsync();
            var res = req.ToAuthTokenResult();
            return res;
        }

        public async Task<CloneItemResult> CloneItem(string fromUrl, string toPath)
        {
            var req = await new CloneItemRequest(_cloudApi, AuthToken.Value.Token, fromUrl, toPath).MakeRequestAsync();
            var res = req.ToCloneItemResult();
            return res;
        }

        public async Task<CopyResult> Copy(string sourceFullPath, string destinationPath, ConflictResolver? conflictResolver = null)
        {
            var req = await new CopyRequest(_cloudApi, AuthToken.Value.Token, sourceFullPath, destinationPath, conflictResolver).MakeRequestAsync();
            var res = req.ToCopyResult();
            return res;
        }

        public async Task<CopyResult> Move(string sourceFullPath, string destinationPath, ConflictResolver? conflictResolver = null)
        {
            var req = await new MoveRequest(_cloudApi, AuthToken.Value.Token, sourceFullPath, destinationPath).MakeRequestAsync();
            var res = req.ToCopyResult();
            return res;
        }

        public async Task<FolderInfoResult> FolderInfo(string path, bool isWebLink = false, int offset = 0, int limit = Int32.MaxValue)
        {
            var req = await new FolderInfoRequest(_cloudApi, AuthToken.Value.Token, path, isWebLink, offset, limit).MakeRequestAsync();
            var res = req;
            return res;
        }

        public async Task<FolderInfoResult> ItemInfo(string path, bool isWebLink = false, int offset = 0, int limit = Int32.MaxValue)
        {
            var req = await new ItemInfoRequest(_cloudApi, AuthToken.Value.Token, path, isWebLink, offset, limit).MakeRequestAsync();
            var res = req;
            return res;
        }

        public async Task<AccountInfoResult> AccountInfo()
        {
            var req = await new AccountInfoRequest(_cloudApi, AuthToken.Value.Token).MakeRequestAsync();
            var res = req.ToAccountInfo();
            return res;
        }

        public async Task<PublishResult> Publish(string fullPath)
        {
            var req = await new PublishRequest(_cloudApi, AuthToken.Value.Token, _cloudApi.Account.Credentials.Login, fullPath).MakeRequestAsync();
            var res = req.ToPublishResult();
            return res;
        }

        public async Task<UnpublishResult> Unpublish(string publicLink)
        {
            var req = await new UnpublishRequest(_cloudApi, AuthToken.Value.Token, publicLink).MakeRequestAsync();
            var res = req.ToUnpublishResult();
            return res;
        }

        public async Task<RemoveResult> Remove(string fullPath)
        {
            var req = await new RemoveRequest(_cloudApi, AuthToken.Value.Token, fullPath).MakeRequestAsync();
            var res = req.ToRemoveResult();
            return res;
        }

        public async Task<RenameResult> Rename(string fullPath, string newName)
        {
            var req = await new RenameRequest(_cloudApi, AuthToken.Value.Token, fullPath, newName).MakeRequestAsync();
            var res = req.ToRenameResult();
            return res;
        }

        public async Task<Dictionary<ShardType, ShardInfo>> ShardInfo()
        {
            var req = await new ShardInfoRequest(_cloudApi, AuthToken.Value.Token).MakeRequestAsync();
            var res = req.ToShardInfo();
            return res;
        }

        public string DownloadToken => _cachedDownloadToken.Value;


        public async Task<CreateFolderResult> CreateFolder(string path)
        {
            return (await new Web.CreateFolderRequest(_cloudApi, AuthToken.Value.Token, path).MakeRequestAsync())
                .ToCreateFolderResult();
        }

        public async Task<AddFileResult> AddFile(string fileFullPath, string fileHash, FileSize fileSize, DateTime dateTime, ConflictResolver? conflictResolver)
        {
            var res = await new Web.CreateFileRequest(_cloudApi, AuthToken.Value.Token, fileFullPath, fileHash, fileSize, conflictResolver)
                .MakeRequestAsync();

            return res.ToAddFileResult();
        }


    }
}