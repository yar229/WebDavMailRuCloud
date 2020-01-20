using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using YaR.Clouds.Base.Repos.MailRuCloud.Mobile.Requests;
using YaR.Clouds.Base.Repos.MailRuCloud.Mobile.Requests.Types;
using YaR.Clouds.Base.Requests;
using YaR.Clouds.Base.Requests.Types;
using YaR.Clouds.Common;
using YaR.Clouds.Extensions;
using YaR.Clouds.Links;

namespace YaR.Clouds.Base.Repos.MailRuCloud.Mobile
{
    /// <summary>
    /// Part of Mobile protocol.
    /// Not usable.
    /// </summary>
    class MobileRequestRepo : MailRuBaseRepo, IRequestRepo
    {
        private static readonly log4net.ILog Logger = log4net.LogManager.GetLogger(typeof(MobileRequestRepo));

        public override HttpCommonSettings HttpSettings { get; } = new HttpCommonSettings
        {
            ClientId = "cloud-win",
            UserAgent = "CloudDiskOWindows 17.12.0009 beta WzBbt1Ygbm"
        };

        public MobileRequestRepo(IWebProxy proxy, IAuth auth, int listDepth)
            :base(new Credentials(auth.Login, auth.Password))
        {
	        _listDepth = listDepth;

			HttpSettings.Proxy = proxy;

            Authent = auth;

            _metaServer = new Cached<ServerRequestResult>(old =>
                {
                    Logger.Debug("MetaServer expired, refreshing.");
                    var server = new MobMetaServerRequest(HttpSettings).MakeRequestAsync().Result;
                    return server;
                },
                value => TimeSpan.FromSeconds(MetaServerExpiresSec));

            //_downloadServer = new Cached<ServerRequestResult>(old =>
            //    {
            //        Logger.Debug("DownloadServer expired, refreshing.");
            //        var server = new GetServerRequest(HttpSettings).MakeRequestAsync().Result;
            //        return server;
            //    },
            //    value => TimeSpan.FromSeconds(DownloadServerExpiresSec));
        }




        private readonly Cached<ServerRequestResult> _metaServer;
        private const int MetaServerExpiresSec = 20 * 60;

        //private readonly Cached<ServerRequestResult> _downloadServer;
		private readonly int _listDepth;
		private const int DownloadServerExpiresSec = 20 * 60;


        

        //public HttpWebRequest UploadRequest(File file, UploadMultipartBoundary boundary)
        //{
        //    throw new NotImplementedException();
        //}

        public Stream GetDownloadStream(File file, long? start = null, long? end = null)
        {
            throw new NotImplementedException();
        }

        //public HttpWebRequest DownloadRequest(long instart, long inend, File file, ShardInfo shard)
        //{
        //    string url = $"{_downloadServer.Value.Url}{Uri.EscapeDataString(file.FullPath)}?token={Authent.AccessToken}&client_id={HttpSettings.ClientId}";

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
            //TODO: implement
            Logger.Warn($"{nameof(MobileRequestRepo)}.{nameof(BanShardInfo)} not implemented");
        }

        public override Task<ShardInfo> GetShardInfo(ShardType shardType)
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

        public async Task<IEntry> FolderInfo(RemotePath path, int offset = 0, int limit = Int32.MaxValue, int depth = 1)
        {
            if (path.IsLink)
                throw new NotImplementedException(nameof(FolderInfo));

            var req = new ListRequest(HttpSettings, Authent, _metaServer.Value.Url, path.Path, _listDepth);
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

        public Task<FolderInfoResult> ItemInfo(RemotePath path, int offset = 0, int limit = Int32.MaxValue)
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

        public Task<UnpublishResult> Unpublish(Uri publicLink, string fullPath = null)
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

            await new MoveRequest(HttpSettings, Authent, _metaServer.Value.Url, fullPath, target)
                .MakeRequestAsync();
            var res = new RenameResult { IsSuccess = true };
            return res;
        }

        public Dictionary<ShardType, ShardInfo> GetShardInfo1()
        {
            throw new NotImplementedException("Mobile GetShardInfo1 not implemented");
        }

        public IEnumerable<PublicLinkInfo> GetShareLinks(string fullPath)
        {
            throw new NotImplementedException("Mobile GetShareLink not implemented");
        }

        public void CleanTrash()
        {
            throw new NotImplementedException();
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