using YaR.MailRuCloud.Api.Base.Requests.Types;

namespace YaR.MailRuCloud.Api.Base.Requests
{
    class ShardInfoRequest : BaseRequest<ShardInfoResult>
    {
        private readonly bool _isAnonymous;

        public ShardInfoRequest(CloudApi cloudApi, bool isAnonymous) : base(cloudApi)
        {
            _isAnonymous = isAnonymous;
        }

        

        public override string RelationalUri
        {
            get
            {
                var uri = string.Format("{0}/api/v2/dispatcher?{2}={1}", ConstSettings.CloudDomain, !_isAnonymous ? CloudApi.Account.AuthToken : 2.ToString(), !_isAnonymous ? "token" : "api");
                return uri;
            }
        }
    }
}
