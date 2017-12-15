using System;
using System.IO;
using System.Net;
using System.Net.Mime;
using System.Threading.Tasks;
using YaR.MailRuCloud.Api.Base.Requests.Types;
using YaR.MailRuCloud.Api.Base.Requests.WebBin;
using YaR.MailRuCloud.Api.Base.Requests.WebBin.Types;
using YaR.MailRuCloud.Api.Base.Threads;
using YaR.MailRuCloud.Api.Extensions;
using YaR.MailRuCloud.Api.Links;

namespace YaR.MailRuCloud.Api.Base.Requests.Repo
{
    class MobileRequestRepo : IRequestRepo
    {
        private static readonly log4net.ILog Logger = log4net.LogManager.GetLogger(typeof(MobileRequestRepo));

        public HttpCommonSettings HttpSettings { get; } = new HttpCommonSettings
        {
            ClientId = "cloud-win",
            UserAgent = "CloudDiskOWindows 17.12.0009 beta WzBbt1Ygbm"
        };

        public int PendingDownloads { get; set; }

        public MobileRequestRepo(IWebProxy proxy, IAuth auth)
        {
            HttpSettings.Proxy = proxy;

            Authent = auth;

            _metaServer = new Cached<MobMetaServerRequest.Result>(old =>
                {
                    Logger.Debug("MetaServer expired, refreshing.");
                    var server = new MobMetaServerRequest(HttpSettings).MakeRequestAsync().Result;
                    return server;
                },
                value => TimeSpan.FromSeconds(MetaServerExpiresSec));

            _downloadServer = new Cached<MobDownloadServerRequest.Result>(old =>
                {
                    Logger.Debug("DownloadServer expired, refreshing.");
                    var server = new MobDownloadServerRequest(HttpSettings).MakeRequestAsync().Result;
                    return server;
                },
                value => TimeSpan.FromSeconds(DownloadServerExpiresSec));
        }




        private readonly Cached<MobMetaServerRequest.Result> _metaServer;
        private const int MetaServerExpiresSec = 20 * 60;

        private readonly Cached<MobDownloadServerRequest.Result> _downloadServer;
        private const int DownloadServerExpiresSec = 20 * 60;


        public IAuth Authent { get; }

        public HttpWebRequest UploadRequest(ShardInfo shard, File file, UploadMultipartBoundary boundary)
        {
            throw new NotImplementedException();
        }

        public Stream GetDownloadStream(File file, long? start = null, long? end = null)
        {
            throw new NotImplementedException();
        }

        public HttpWebRequest DownloadRequest(long instart, long inend, File file, ShardInfo shard)
        {
            string url = $"{_downloadServer.Value.Url}{Uri.EscapeDataString(file.FullPath)}?token={Authent.AccessToken}&client_id={HttpSettings.ClientId}";

            var request = (HttpWebRequest)WebRequest.Create(url);

            request.Headers.Add("Accept-Ranges", "bytes");
            request.AddRange(instart, inend);
            request.Proxy = HttpSettings.Proxy;
            request.CookieContainer = Authent.Cookies;
            request.Method = "GET";
            request.ContentType = MediaTypeNames.Application.Octet;
            request.Accept = "*/*";
            request.UserAgent = HttpSettings.UserAgent;
            request.AllowReadStreamBuffering = false;

            request.Timeout = 15 * 1000;

            return request;
        }


        public void BanShardInfo(ShardInfo banShard)
        {
            //TODO: implement
            Logger.Warn($"{nameof(MobileRequestRepo)}.{nameof(BanShardInfo)} not implemented");
        }

        public Task<ShardInfo> GetShardInfo(ShardType shardType)
        {
            //TODO: must hide shard functionality into repo after DownloadStream and UploadStream refact

            var shi = new ShardInfo
            {
                Url = _metaServer.Value.Url,
                Type = shardType
            };

            return Task.FromResult(shi);
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

        public async Task<IEntry> FolderInfo(string path, Link ulink, int offset = 0, int limit = Int32.MaxValue)
        {
            var req = new ListRequest(HttpSettings, Authent, _metaServer.Value.Url, path) { Depth = 1};
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

        public async Task<AccountInfoResult> AccountInfo()
        {
            var req = await new AccountInfoRequest(HttpSettings, Authent).MakeRequestAsync();
            var res = req.ToAccountInfo();
            return res;
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

        public async Task<RenameResult> Rename(string fullPath, string newName)
        {
            string target = WebDavPath.Combine(WebDavPath.Parent(fullPath), newName);

            await new RenameRequest(HttpSettings, Authent, _metaServer.Value.Url, fullPath, target)
                .MakeRequestAsync();
            var res = new RenameResult { IsSuccess = true };
            return res;
        }

        public async Task<CreateFolderResult> CreateFolder(string path)
        {
            return (await new CreateFolderRequest(HttpSettings, Authent, _metaServer.Value.Url, path).MakeRequestAsync())
                .ToCreateFolderResult();
        }

        public async Task<AddFileResult> AddFile(string fileFullPath, string fileHash, FileSize fileSize, DateTime dateTime, ConflictResolver? conflictResolver)
        {
            var res = await new MobAddFileRequest(HttpSettings, Authent, _metaServer.Value.Url,
                    fileFullPath, fileHash, fileSize, dateTime, conflictResolver)
                .MakeRequestAsync();

            return res.ToAddFileResult();
        }


    }
}