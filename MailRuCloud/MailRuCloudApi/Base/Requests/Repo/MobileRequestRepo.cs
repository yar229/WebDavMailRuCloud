using System;
using System.Threading.Tasks;
using YaR.MailRuCloud.Api.Base.Requests.Mobile;
using YaR.MailRuCloud.Api.Base.Requests.Types;
using YaR.MailRuCloud.Api.Extensions;

namespace YaR.MailRuCloud.Api.Base.Requests.Repo
{
    class MobileRequestRepo : IRequestRepo
    {
        private static readonly log4net.ILog Logger = log4net.LogManager.GetLogger(typeof(MobileRequestRepo));
        private readonly CloudApi _cloudApi;

        public MobileRequestRepo(CloudApi cloudApi)
        {
            _cloudApi = cloudApi;

            _authTokenMobile = new Cached<string>(() =>
                {
                    Logger.Debug("AuthTokenMobile expired, refreshing.");
                    var token = new MobAuthRequest(_cloudApi).MakeRequestAsync().Result.ToToken();
                    return token;
                },
                TimeSpan.FromSeconds(AuthTokenMobileExpiresInSec));

            _metaServer = new Cached<MobMetaServerRequest.Result>(() =>
                {
                    Logger.Debug("MetaServer expired, refreshing.");
                    var server = new MobMetaServerRequest(_cloudApi).MakeRequestAsync().Result;
                    return server;
                },
                TimeSpan.FromSeconds(MetaServerExpiresSec));
        }


        /// <summary>
        /// Token for authorization in mobile version
        /// </summary>
        private readonly Cached<string> _authTokenMobile;
        private const int AuthTokenMobileExpiresInSec = 58 * 60;

        private readonly Cached<MobMetaServerRequest.Result> _metaServer;
        private const int MetaServerExpiresSec = 20 * 60;


        public async Task<CreateFolderResult> CreateFolder(string path)
        {
            return (await new Mobile.CreateFolderRequest(_cloudApi, _authTokenMobile.Value, _metaServer.Value.Url,
                    path).MakeRequestAsync())
                .ToCreateFolderResult();
        }

        public async Task<AddFileResult> AddFile(string fileFullPath, string fileHash, FileSize fileSize, DateTime dateTime, ConflictResolver? conflictResolver)
        {
            var res = await new Mobile.MobAddFileRequest(_cloudApi, _authTokenMobile.Value, _metaServer.Value.Url,
                    fileFullPath, fileHash, fileSize, dateTime)
                .MakeRequestAsync();

            return res.ToAddFileResult();
        }
    }
}