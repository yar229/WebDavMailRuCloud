using System;
using System.Collections.Generic;
using YaR.MailRuCloud.Api.Base.Auth;
using YaR.MailRuCloud.Api.Base.Requests;
using YaR.MailRuCloud.Api.Base.Requests.Types;
using YaR.MailRuCloud.Api.Base.Requests.WebM1;
using YaR.MailRuCloud.Api.Common;
using YaR.MailRuCloud.Api.Extensions;

namespace YaR.MailRuCloud.Api.Base.Repos
{
    class ShardManager
    {
        //TODO: refact required

        private static readonly log4net.ILog Logger = log4net.LogManager.GetLogger(typeof(ShardManager));

        public ShardManager(HttpCommonSettings httpsettings, IAuth auth)
        {
            _metaServer = new Cached<Requests.WebBin.MobMetaServerRequest.Result>(old =>
                {
                    Logger.Debug("Requesting new meta server");
                    var server = new Requests.WebBin.MobMetaServerRequest(httpsettings).MakeRequestAsync().Result;
                    return server;
                },
                value => TimeSpan.FromSeconds(MetaServerExpiresSec));

            BannedShards = new Cached<List<ShardInfo>>(old => new List<ShardInfo>(),
                value => TimeSpan.FromMinutes(2));

            CachedShards = new Cached<Dictionary<ShardType, ShardInfo>>(old => new ShardInfoRequest(httpsettings, auth).MakeRequestAsync().Result.ToShardInfo(),
                value => TimeSpan.FromSeconds(ShardsExpiresInSec));

            DownloadServersPending = new Pending<Cached<Requests.WebBin.ServerRequest.Result>>(8,
                () => new Cached<Requests.WebBin.ServerRequest.Result>(old =>
                    {
                        var server = new Requests.WebBin.GetServerRequest(httpsettings).MakeRequestAsync().Result;
                        Logger.Debug($"Download server changed to {server.Url}");
                        return server;
                    },
                    value => TimeSpan.FromSeconds(DownloadServerExpiresSec)
                ));

            WeblinkDownloadServersPending = new Pending<Cached<Requests.WebBin.ServerRequest.Result>>(8,
                () => new Cached<Requests.WebBin.ServerRequest.Result>(old =>
                    {
                        var server = new Requests.WebBin.WeblinkGetServerRequest(httpsettings).MakeRequestAsync().Result;
                        Logger.Debug($"weblink Download server changed to {server.Url}");
                        return server;
                    },
                    value => TimeSpan.FromSeconds(DownloadServerExpiresSec)
                ));
        }


        public Pending<Cached<Requests.WebBin.ServerRequest.Result>> DownloadServersPending { get; }
        public Pending<Cached<Requests.WebBin.ServerRequest.Result>> WeblinkDownloadServersPending { get; }

        public ShardInfo MetaServer => new ShardInfo {Url = _metaServer.Value.Url, Count = _metaServer.Value.Unknown};
        private readonly Cached<Requests.WebBin.MobMetaServerRequest.Result> _metaServer;

        public Cached<Dictionary<ShardType, ShardInfo>> CachedShards { get; }

        public Cached<List<ShardInfo>> BannedShards { get; }


        private const int ShardsExpiresInSec = 30 * 60;
        private const int MetaServerExpiresSec = 20 * 60;
        private const int DownloadServerExpiresSec = 3 * 60;
    }
}
