using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using YaR.MailRuCloud.Api.Base.Requests.Mobile;
using YaR.MailRuCloud.Api.Base.Requests.Mobile.Types;
using YaR.MailRuCloud.Api.Base.Requests.Types;
using YaR.MailRuCloud.Api.Extensions;
using YaR.MailRuCloud.Api.Links;

namespace YaR.MailRuCloud.Api.Base.Requests.Repo
{
    class MobileRequestRepo : IRequestRepo
    {
        private static readonly log4net.ILog Logger = log4net.LogManager.GetLogger(typeof(MobileRequestRepo));
        private readonly CloudApi _cloudApi;
        private readonly RequestInit _init;

        public MobileRequestRepo(CloudApi cloudApi)
        {
            _cloudApi = cloudApi;

            _authTokenMobile = new Cached<AuthTokenResult>(() =>
                {
                    Logger.Debug("AuthTokenMobile expired, refreshing.");
                    var token = Auth().Result;
                    return token;
                },
                TimeSpan.FromSeconds(AuthTokenMobileExpiresInSec));

            _init = new RequestInit(_cloudApi.Account.Proxy, _cloudApi.Account.Cookies, _authTokenMobile, _cloudApi.Account.Credentials.Login);

            _metaServer = new Cached<MobMetaServerRequest.Result>(() =>
                {
                    Logger.Debug("MetaServer expired, refreshing.");
                    var server = new MobMetaServerRequest(_init).MakeRequestAsync().Result;
                    return server;
                },
                TimeSpan.FromSeconds(MetaServerExpiresSec));
        }


        /// <summary>
        /// Token for authorization in mobile version
        /// </summary>
        private readonly Cached<AuthTokenResult> _authTokenMobile;
        private const int AuthTokenMobileExpiresInSec = 58 * 60;

        private readonly Cached<MobMetaServerRequest.Result> _metaServer;
        private const int MetaServerExpiresSec = 20 * 60;


        public Task<bool> Login(Account.AuthCodeRequiredDelegate onAuthCodeRequired)
        {
            throw new NotImplementedException();
        }

        public void BanShardInfo(ShardInfo banShard)
        {
            throw new NotImplementedException();
        }

        public Task<ShardInfo> GetShardInfo(ShardType shardType)
        {
            throw new NotImplementedException();
        }



        public async Task<AuthTokenResult> Auth()
        {
            var init = new RequestInit(_cloudApi.Account.Proxy, _cloudApi.Account.Cookies, _authTokenMobile, _cloudApi.Account.Credentials.Login);
            var req = await new MobAuthRequest(init, _cloudApi.Account.Credentials.Password).MakeRequestAsync();
            var res = req.ToAuthTokenResult();
            return res;
        }

        public Task<CloneItemResult> CloneItem(string fromUrl, string toPath)
        {
            throw new NotImplementedException();
        }

        public Task<CopyResult> Copy(string sourceFullPath, string destinationPath, ConflictResolver? conflictResolver = null)
        {
            throw new NotImplementedException();
        }

        public Task<CopyResult> Move(string sourceFullPath, string destinationPath, ConflictResolver? conflictResolver = null)
        {
            throw new NotImplementedException();
        }

        //public async Task<FolderInfoResult> FolderInfo(string path, bool isWebLink = false, int offset = 0, int limit = Int32.MaxValue)
        public async Task<IEntry> FolderInfo(string path, Link ulink, bool isWebLink = false, int offset = 0, int limit = Int32.MaxValue)
        {
            var req = new ListRequest(_init, _metaServer.Value.Url, path) { Depth = 1};
            var res = await req.MakeRequestAsync();

            if (res.Item is FsFolder fsf)
            {
                var f = new Folder(fsf.Size == null ? 0 : (long)fsf.Size.Value, fsf.FullPath);
                foreach (var fsi in fsf.Items)
                {
                    if (fsi is FsFile fsfi)
                    {
                        var fi = new File(fsfi.FullPath, (long)fsfi.Size, fsfi.Sha1.ToHexString())
                        {
                            CreationTimeUtc = fsfi.ModifDate,
                            LastWriteTimeUtc = fsfi.ModifDate,
                        };
                        f.Files.Add(fi);
                    }
                    else if (fsi is FsFolder fsfo)
                    {
                        var fo = new Folder(fsfo.Size == null ? 0 : (long) fsfo.Size.Value, fsfo.FullPath);
                        f.Folders.Add(fo);
                    }
                    else throw new Exception($"Unknown item type {fsi.GetType()}");
                }
                return f;
            }

            if (res.Item is FsFile fsfi1)
            {
                var fi = new File(fsfi1.FullPath, (long)fsfi1.Size, fsfi1.Sha1.ToHexString())
                {
                    CreationTimeUtc = fsfi1.ModifDate,
                    LastWriteTimeUtc = fsfi1.ModifDate,
                };

                return fi;
            }

            return null;
        }

        public Task<FolderInfoResult> ItemInfo(string path, bool isWebLink = false, int offset = 0, int limit = Int32.MaxValue)
        {
            throw new NotImplementedException();
        }

        public Task<AccountInfoResult> AccountInfo()
        {
            throw new NotImplementedException();
        }

        public Task<PublishResult> Publish(string fullPath)
        {
            throw new NotImplementedException();
        }

        public Task<UnpublishResult> Unpublish(string publicLink)
        {
            throw new NotImplementedException();
        }

        public Task<RemoveResult> Remove(string fullPath)
        {
            throw new NotImplementedException();
        }

        public Task<RenameResult> Rename(string fullPath, string newName)
        {
            throw new NotImplementedException();
        }

        public Task<Dictionary<ShardType, ShardInfo>> ShardInfo()
        {
            throw new NotImplementedException();
        }

        public string DownloadToken => throw new NotImplementedException();

        public async Task<CreateFolderResult> CreateFolder(string path)
        {
            return (await new Mobile.CreateFolderRequest(_init, _metaServer.Value.Url, path).MakeRequestAsync())
                .ToCreateFolderResult();
        }

        public async Task<AddFileResult> AddFile(string fileFullPath, string fileHash, FileSize fileSize, DateTime dateTime, ConflictResolver? conflictResolver)
        {
            var res = await new Mobile.MobAddFileRequest(_init, _metaServer.Value.Url,
                    fileFullPath, fileHash, fileSize, dateTime)
                .MakeRequestAsync();

            return res.ToAddFileResult();
        }


    }
}