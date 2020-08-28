using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mime;
using System.Threading;
using System.Threading.Tasks;
using YaR.Clouds.Base.Repos.MailRuCloud.Mobile.Requests;
using YaR.Clouds.Base.Repos.MailRuCloud.Mobile.Requests.Types;
using YaR.Clouds.Base.Repos.MailRuCloud.WebM1.Requests;
using YaR.Clouds.Base.Requests;
using YaR.Clouds.Base.Requests.Types;
using YaR.Clouds.Base.Streams;
using YaR.Clouds.Common;
using AnonymousRepo = YaR.Clouds.Base.Repos.MailRuCloud.WebV2.WebV2RequestRepo;
using AccountInfoRequest = YaR.Clouds.Base.Repos.MailRuCloud.WebM1.Requests.AccountInfoRequest;
using CreateFolderRequest = YaR.Clouds.Base.Repos.MailRuCloud.Mobile.Requests.CreateFolderRequest;
using MoveRequest = YaR.Clouds.Base.Repos.MailRuCloud.Mobile.Requests.MoveRequest;

namespace YaR.Clouds.Base.Repos.MailRuCloud.WebBin
{
    /// <summary>
    /// Combination of WebM1 and Mobile protocols
    /// </summary>
    class WebBinRequestRepo : MailRuBaseRepo, IRequestRepo
    {
        private static readonly log4net.ILog Logger = log4net.LogManager.GetLogger(typeof(WebBinRequestRepo));

        private readonly AuthCodeRequiredDelegate _onAuthCodeRequired;

        protected ShardManager ShardManager => _shardManager ??= new ShardManager(this);
        private ShardManager _shardManager;

        protected IRequestRepo AnonymousRepo => _anonymousRepo ??= new AnonymousRepo(HttpSettings.Proxy, Credentials,
            _onAuthCodeRequired);
        private IRequestRepo _anonymousRepo;


        public sealed override HttpCommonSettings HttpSettings { get; } = new HttpCommonSettings
        {
            ClientId = "cloud-win",
            UserAgent = "CloudDiskOWindows 17.12.0009 beta WzBbt1Ygbm"
        };

        public WebBinRequestRepo(IWebProxy proxy, IBasicCredentials creds, AuthCodeRequiredDelegate onAuthCodeRequired)
            :base(creds)
        {
            _onAuthCodeRequired = onAuthCodeRequired;

			ServicePointManager.DefaultConnectionLimit = int.MaxValue;

            HttpSettings.Proxy = proxy;
            Authent = new OAuth(HttpSettings, creds, onAuthCodeRequired);
        }

        

        public Stream GetDownloadStream(File afile, long? start = null, long? end = null)
        {
            var istream = GetDownloadStreamInternal(afile, start, end);
            return istream;
        }

        private DownloadStream GetDownloadStreamInternal(File afile, long? start = null, long? end = null)
        {
            bool isLinked = afile.PublicLinks.Any();

            Cached<ServerRequestResult> downServer = null;
            var pendingServers = isLinked
                ? ShardManager.WeblinkDownloadServersPending
                : ShardManager.DownloadServersPending;
            Stopwatch watch = new Stopwatch();

            HttpWebRequest request = null;
            CustomDisposable<HttpWebResponse> ResponseGenerator(long instart, long inend, File file)
            {
                var resp = Retry.Do(() =>
                {
                    downServer = pendingServers.Next(downServer);

                    string url;

                    if (isLinked)
                    {
                        var urii = file.PublicLinks.First().Uri;
                        var uriistr = urii.OriginalString;
                        var baseura = PublicBaseUrls.First(pbu => uriistr.StartsWith(pbu, StringComparison.InvariantCulture));
                        if (string.IsNullOrEmpty(baseura))
                            throw new ArgumentException("url does not starts with base url");

                        url = $"{downServer.Value.Url}{WebDavPath.EscapeDataString(uriistr.Remove(0, baseura.Length))}";
                    }
                    else
                    {
                        url = $"{downServer.Value.Url}{Uri.EscapeDataString(file.FullPath.TrimStart('/'))}";
                    }

                    url += $"?client_id={HttpSettings.ClientId}&token={Authent.AccessToken}";

                    //string url =(isLinked
                    //        ? $"{downServer.Value.Url}{WebDavPath.EscapeDataString(file.PublicLinks.First().Uri.PathAndQuery)}"
                    //        : $"{downServer.Value.Url}{Uri.EscapeDataString(file.FullPath.TrimStart('/'))}") +
                    //    $"?client_id={HttpSettings.ClientId}&token={Authent.AccessToken}";
                    var uri = new Uri(url);

                    request = (HttpWebRequest) WebRequest.Create(uri.OriginalString);

                    request.AddRange(instart, inend);
                    request.Proxy = HttpSettings.Proxy;
                    request.CookieContainer = Authent.Cookies;
                    request.Method = "GET";
                    request.Accept = "*/*";
                    request.UserAgent = HttpSettings.UserAgent;
                    request.Host = uri.Host;
                    request.AllowWriteStreamBuffering = false;

                    if (isLinked)
                    {
                        request.Headers.Add("Accept-Ranges", "bytes");
                        request.ContentType = MediaTypeNames.Application.Octet;
                        request.Referer = $"{ConstSettings.CloudDomain}/home/{Uri.EscapeDataString(file.Path)}";
                        request.Headers.Add("Origin", ConstSettings.CloudDomain);
                    }

                    request.Timeout = 15 * 1000;
                    request.ReadWriteTimeout = 15 * 1000;

                    watch.Start();
                    var response = (HttpWebResponse)request.GetResponse();
                    return new CustomDisposable<HttpWebResponse>
                    {
                        Value = response,
                        OnDispose = () =>
                        {
                            pendingServers.Free(downServer);
                            watch.Stop();
                            Logger.Debug($"HTTP:{request.Method}:{request.RequestUri.AbsoluteUri} ({watch.Elapsed.Milliseconds} ms)");
                        }
                    };
                },
                exception => 
                    ((exception as WebException)?.Response as HttpWebResponse)?.StatusCode == HttpStatusCode.NotFound,
                exception =>
                {
                    pendingServers.Free(downServer);
                    Logger.Warn($"Retrying HTTP:{request.Method}:{request.RequestUri.AbsoluteUri} on exception {exception.Message}");
                },
                TimeSpan.FromSeconds(1), 2);

                return resp;
            }

            var stream = new DownloadStream(ResponseGenerator, afile, start, end);
            return stream;
        }


        //public HttpWebRequest UploadRequest(File file, UploadMultipartBoundary boundary)
        //{
        //    var shard = GetShardInfo(ShardType.Upload).Result;
        //    var url = new Uri($"{shard.Url}?client_id={HttpSettings.ClientId}&token={Authent.AccessToken}");

        //    var request = (HttpWebRequest)WebRequest.Create(url);
        //    request.Proxy = HttpSettings.Proxy;
        //    request.CookieContainer = Authent.Cookies;
        //    request.Method = "PUT";
        //    request.ContentLength = file.OriginalSize;
        //    request.Accept = "*/*";
        //    request.UserAgent = HttpSettings.UserAgent;
        //    request.AllowWriteStreamBuffering = false;

        //    request.Timeout = 15 * 1000;
        //    request.ReadWriteTimeout = 15 * 1000;
        //    //request.ServicePoint.ConnectionLimit = int.MaxValue;

        //    return request;
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
                var ishards = await Task.Run(() => ShardManager.CachedShards.Value);
                var ishard = ishards[shardType];
                var banned = ShardManager.BannedShards.Value;
                if (banned.All(bsh => bsh.Url != ishard.Url))
                {
                    if (refreshed) Authent.ExpireDownloadToken();
                    return ishard;
                }
                ShardManager.CachedShards.Expire();
                refreshed = true;
            }

            Logger.Error("Cannot get working shard.");

            var shards = await Task.Run(() => ShardManager.CachedShards.Value);
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
            //var req = await new MoveRequest(HttpSettings, Authent, sourceFullPath, destinationPath).MakeRequestAsync();
            //var res = req.ToCopyResult();
            //return res;

            var req = await new MoveRequest(HttpSettings, Authent, ShardManager.MetaServer.Url, sourceFullPath, destinationPath)
                .MakeRequestAsync();

            var res = req.ToCopyResult(WebDavPath.Name(destinationPath));
            return res;

        }

        private async Task<IEntry> FolderInfo(string path, int depth = 1)
        {
            ListRequest.Result datares;
            try
            {
                datares = await new ListRequest(HttpSettings, Authent, ShardManager.MetaServer.Url, path, depth)
                    .MakeRequestAsync();

                // если файл разбит или зашифрован - то надо взять все куски
                // в протоколе V2 на запрос к файлу сразу приходит листинг каталога, в котором он лежит
                // здесь (протокол Bin) приходит информация именно по указанному файлу
                // поэтому вот такой костыль с двойным запросом
                //TODO: переделать двойной запрос к файлу
                if (datares.Item is FsFile fsfile && fsfile.Size < 2048)
                {
                    string name = WebDavPath.Name(path);
                    path = WebDavPath.Parent(path);

                    datares = await new ListRequest(HttpSettings, Authent, ShardManager.MetaServer.Url, path, 1)
                        .MakeRequestAsync();

                    var zz = datares.ToFolder();

                    return zz.Files.First(f => f.Name == name);
                }
            }
            catch (RequestException re) when (re.StatusCode ==  HttpStatusCode.NotFound)
            {
                return null;
            }
            catch (WebException e) when ((e.Response as HttpWebResponse)?.StatusCode == HttpStatusCode.NotFound)
            {
                return null;
            }

            var z = datares.ToEntry();

            return z;
        }

        public async Task<IEntry> FolderInfo(RemotePath path, int offset = 0, int limit = Int32.MaxValue, int depth = 1)
        {
            if (Credentials.IsAnonymous)
                return await AnonymousRepo.FolderInfo(path, offset, limit);

            if (!path.IsLink && depth > 1)
                return await FolderInfo(path.Path, depth);

            FolderInfoResult datares;
            try
            {
                datares = await new FolderInfoRequest(HttpSettings, Authent, path, offset, limit)
                    .MakeRequestAsync();
            }
            catch (WebException e) when ((e.Response as HttpWebResponse)?.StatusCode == HttpStatusCode.NotFound)
            {
                return null;
            }

            Cloud.ItemType itemType;

            //TODO: subject to refact, bad-bad-bad
            if (!path.IsLink || path.Link.ItemType == Cloud.ItemType.Unknown)
                itemType = datares.Body.Home == path.Path ||
                           WebDavPath.PathEquals("/" + datares.Body.Weblink, path.Path)
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
                    nameReplacement: path.Link?.IsLinkedToFileSystem ?? true ? WebDavPath.Name(path.Path) : path.Link.Name )
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

            if (res.IsSuccess)
            {
                CachedSharedList.Value[fullPath] = new [] {new PublicLinkInfo(PublicBaseUrlDefault + res.Url)};
            }

            return res;
        }

        public async Task<UnpublishResult> Unpublish(Uri publicLink, string fullPath)
        {
            foreach (var item in CachedSharedList.Value
                .Where(kvp => kvp.Value.Any(u => u.Uri.Equals(publicLink))).ToList())
            {
                CachedSharedList.Value.Remove(item.Key);
            }

            var req = await new UnpublishRequest(this, HttpSettings, Authent, publicLink.OriginalString).MakeRequestAsync();
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
            //var req = await new RenameRequest(HttpSettings, Authent, fullPath, newName).MakeRequestAsync();
            //var res = req.ToRenameResult();
            //return res;

            string newFullPath = WebDavPath.Combine(WebDavPath.Parent(fullPath), newName);
            var req = await new MoveRequest(HttpSettings, Authent, ShardManager.MetaServer.Url, fullPath, newFullPath)
                .MakeRequestAsync();

            var res = req.ToRenameResult();
            return res;
        }

        public Dictionary<ShardType, ShardInfo> GetShardInfo1()
        {
            if (Authent.IsAnonymous)
                return new Clouds.Base.Repos.MailRuCloud.WebV2.Requests.ShardInfoRequest(HttpSettings, Authent).MakeRequestAsync().Result.ToShardInfo();


            return new ShardInfoRequest(HttpSettings, Authent).MakeRequestAsync().Result.ToShardInfo();
        }


        public Cached<Dictionary<string, IEnumerable<PublicLinkInfo>>> CachedSharedList
        {
            get
            {
                return _cachedSharedList ??= new Cached<Dictionary<string, IEnumerable<PublicLinkInfo>>>(old =>
                    {
                        var z = GetShareListInner().Result;

                        var res = z.Body.List
                            .ToDictionary(
                                fik => fik.Home,
                                fiv => Enumerable.Repeat(new PublicLinkInfo(PublicBaseUrlDefault + fiv.Weblink),
                                    1));

                        return res;
                    },
                    value => TimeSpan.FromSeconds(30));
            }
        }
        private Cached<Dictionary<string, IEnumerable<PublicLinkInfo>>> _cachedSharedList;

        private async Task<FolderInfoResult> GetShareListInner()
        {
            var res = await new SharedListRequest(HttpSettings, Authent)
                .MakeRequestAsync();

            return res;
        }

        public IEnumerable<PublicLinkInfo> GetShareLinks(string path)
        {
            if (CachedSharedList.Value.TryGetValue(path, out var links))
                foreach (var link in links)
                    yield return link;
        }

        public void CleanTrash()
        {
            throw new NotImplementedException();
        }

        public async Task<CreateFolderResult> CreateFolder(string path)
        {
            //return (await new CreateFolderRequest(HttpSettings, Authent, path).MakeRequestAsync())
            //    .ToCreateFolderResult();

            return (await new CreateFolderRequest(HttpSettings, Authent, ShardManager.MetaServer.Url, path).MakeRequestAsync())
                .ToCreateFolderResult();
        }

        public async Task<AddFileResult> AddFile(string fileFullPath, string fileHash, FileSize fileSize, DateTime dateTime, ConflictResolver? conflictResolver)
        {
            //var res = await new CreateFileRequest(Proxy, Authent, fileFullPath, fileHash, fileSize, conflictResolver)
            //    .MakeRequestAsync();
            //return res.ToAddFileResult();

            //using Mobile request because of supporting file modified time

            //TODO: refact, make mixed repo
            var req = await new MobAddFileRequest(HttpSettings, Authent, ShardManager.MetaServer.Url, fileFullPath, fileHash, fileSize, dateTime, conflictResolver)
                .MakeRequestAsync();

            var res = req.ToAddFileResult();
            return res;
        }
    }
}

