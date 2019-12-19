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

            _metaServer = new Cached<ServerRequestResult>(old =>
                {
                    Logger.Debug("Requesting new meta server");
                    var server =  new MobMetaServerRequest(httpsettings).MakeRequestAsync().Result;
                    return server;
                },
                value => TimeSpan.FromSeconds(MetaServerExpiresSec));

            BannedShards = new Cached<List<ShardInfo>>(old => new List<ShardInfo>(),
                value => TimeSpan.FromMinutes(2));

            //CachedShards = new Cached<Dictionary<ShardType, ShardInfo>>(old => new ShardInfoRequest(httpsettings, auth).MakeRequestAsync().Result.ToShardInfo(),
            //    value => TimeSpan.FromSeconds(ShardsExpiresInSec));

            CachedShards = new Cached<Dictionary<ShardType, ShardInfo>>(old => repo.GetShardInfo1(),
                value => TimeSpan.FromSeconds(ShardsExpiresInSec));

            DownloadServersPending = new Pending<Cached<ServerRequestResult>>(8,
                () => new Cached<ServerRequestResult>(old =>
                    {
                        var server = new GetServerRequest(httpsettings).MakeRequestAsync().Result;
                        Logger.Debug($"Download server changed to {server.Url}");
                        return server;
                    },
                    value => TimeSpan.FromSeconds(DownloadServerExpiresSec)
                ));

            WeblinkDownloadServersPending = new Pending<Cached<ServerRequestResult>>(8,
                () => new Cached<ServerRequestResult>(old =>
                    {
                        var server = new WeblinkGetServerRequest(httpsettings).MakeRequestAsync().Result;
                        Logger.Debug($"weblink Download server changed to {server.Url}");
                        return server;
                    },
                    value => TimeSpan.FromSeconds(DownloadServerExpiresSec)
                ));
        }


        public Pending<Cached<ServerRequestResult>> DownloadServersPending { get; }
        public Pending<Cached<ServerRequestResult>> WeblinkDownloadServersPending { get; }

        public ShardInfo MetaServer => new ShardInfo {Url = _metaServer.Value.Url, Count = _metaServer.Value.Unknown};
        private readonly Cached<ServerRequestResult> _metaServer;

        public Cached<Dictionary<ShardType, ShardInfo>> CachedShards { get; }

        public Cached<List<ShardInfo>> BannedShards { get; }


        private const int ShardsExpiresInSec = 30 * 60;
        private const int MetaServerExpiresSec = 20 * 60;
        private const int DownloadServerExpiresSec = 3 * 60;
    }
}
