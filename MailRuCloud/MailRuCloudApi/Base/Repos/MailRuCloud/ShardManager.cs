using System;
using System.Collections.Generic;
using YaR.Clouds.Base.Repos.MailRuCloud.Mobile.Requests;
using YaR.Clouds.Base.Requests.Types;
using YaR.Clouds.Common;

namespace YaR.Clouds.Base.Repos.MailRuCloud
{
    class ShardManager
    {
        //TODO: refact required

        private static readonly log4net.ILog Logger = log4net.LogManager.GetLogger(typeof(ShardManager));

        public ShardManager(IRequestRepo repo)
        {
            var httpsettings = repo.HttpSettings;

            _metaServer = new Cached<ServerRequestResult>(_ =>
                {
                    Logger.Debug("Requesting new meta server");
                    var server =  new MobMetaServerRequest(httpsettings).MakeRequestAsync().Result;
                    return server;
                },
                _ => TimeSpan.FromSeconds(MetaServerExpiresSec));

            BannedShards = new Cached<List<ShardInfo>>(_ => new List<ShardInfo>(),
                _ => TimeSpan.FromMinutes(2));

            //CachedShards = new Cached<Dictionary<ShardType, ShardInfo>>(old => new ShardInfoRequest(httpsettings, auth).MakeRequestAsync().Result.ToShardInfo(),
            //    value => TimeSpan.FromSeconds(ShardsExpiresInSec));

            CachedShards = new Cached<Dictionary<ShardType, ShardInfo>>(_ => repo.GetShardInfo1(),
                _ => TimeSpan.FromSeconds(ShardsExpiresInSec));

            DownloadServersPending = new Pending<Cached<ServerRequestResult>>(8,
                () => new Cached<ServerRequestResult>(_ =>
                    {
                        var server = new GetServerRequest(httpsettings).MakeRequestAsync().Result;
                        Logger.Debug($"Download server changed to {server.Url}");
                        return server;
                    },
                    _ => TimeSpan.FromSeconds(DownloadServerExpiresSec)
                ));

            UploadServer = new Cached<ShardInfo>(_ =>
                {
                    var server = new GetUploadServerRequest(httpsettings).MakeRequestAsync().Result;
                    Logger.Debug($"Upload server changed to {server.Url}");
                    return new ShardInfo { Count = 0, Type = ShardType.Upload, Url = server.Url };
                },
                _ => TimeSpan.FromSeconds(ShardsExpiresInSec));


            WeblinkDownloadServersPending = new Pending<Cached<ServerRequestResult>>(8,
                () => new Cached<ServerRequestResult>(_ =>
                    {
                        var data = new WeblinkGetServerRequest(httpsettings).MakeRequestAsync().Result;
                        var serverUrl = data.Body.WeblinkGet[0].Url;
                        Logger.Debug($"weblink Download server changed to {serverUrl}");
                        var res = new ServerRequestResult { Url = serverUrl };
                        return res;
                    },
                    _ => TimeSpan.FromSeconds(DownloadServerExpiresSec)
                ));
        }


        public Pending<Cached<ServerRequestResult>> DownloadServersPending { get; }
        public Pending<Cached<ServerRequestResult>> WeblinkDownloadServersPending { get; }

        public ShardInfo MetaServer => new() {Url = _metaServer.Value.Url, Count = _metaServer.Value.Unknown};
        private readonly Cached<ServerRequestResult> _metaServer;

        public Cached<Dictionary<ShardType, ShardInfo>> CachedShards { get; }
        public Cached<ShardInfo> UploadServer { get; }

        public Cached<List<ShardInfo>> BannedShards { get; }


        private const int ShardsExpiresInSec = 30 * 60;
        private const int MetaServerExpiresSec = 20 * 60;
        private const int DownloadServerExpiresSec = 3 * 60;
    }
}
