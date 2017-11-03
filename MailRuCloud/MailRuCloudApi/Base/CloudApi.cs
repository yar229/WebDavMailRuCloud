using System;
using System.Collections.Generic;
using System.Security.Authentication;
using System.Threading;
using System.Threading.Tasks;
using YaR.MailRuCloud.Api.Base.Requests;
using YaR.MailRuCloud.Api.Base.Requests.Types;
using YaR.MailRuCloud.Api.Extensions;

namespace YaR.MailRuCloud.Api.Base
{
    public class CloudApi : IDisposable
    {
        private static readonly log4net.ILog Logger = log4net.LogManager.GetLogger(typeof(CloudApi));

        /// <summary>
        /// Async tasks cancelation token.
        /// </summary>
        public readonly CancellationTokenSource CancelToken = new CancellationTokenSource();


        /// <summary>
        /// Gets or sets account to connect with cloud.
        /// </summary>
        /// <value>Account info.</value>
        public Account Account { get; }

        public CloudApi(string login, string password, ITwoFaHandler twoFaHandler)
        {
            Account = new Account(this, login, password, twoFaHandler);
            if (!Account.Login())
            {
                throw new AuthenticationException("Auth token has't been retrieved.");
            }

            //_cachedShards = new Cached<Dictionary<ShardType, ShardInfo>>(() => new ShardInfoRequest(this).MakeRequestAsync().Result.ToShardInfo(),
            //    TimeSpan.FromMinutes(2));
        }

        ///// <summary>
        ///// Get shard info that to do post get request. Can be use for anonymous user.
        ///// </summary>
        ///// <param name="shardType">Shard type as numeric type.</param>
        ///// <returns>Shard info.</returns>
        //public async Task<ShardInfo> GetShardInfo(ShardType shardType)
        //{
        //    var shards = await Task.Run(() => _cachedShards.Value);
        //    var shard = shards[shardType];
        //    return shard;
        //}

        //private readonly Cached<Dictionary<ShardType, ShardInfo>> _cachedShards;

        #region IDisposable Support
        private bool _disposedValue;

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    CancelToken.Dispose();
                }
                _disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }
        #endregion
    }
}
