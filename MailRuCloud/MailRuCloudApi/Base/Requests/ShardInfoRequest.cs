using YaR.MailRuCloud.Api.Base.Requests.Types;

namespace YaR.MailRuCloud.Api.Base.Requests
{
    class ShardInfoRequest : BaseRequest<ShardInfoResult>
    {
        public ShardInfoRequest(CloudApi cloudApi) : base(cloudApi)
        {
        }

        protected override string RelationalUri
        {
            get
            {
                //var uri = string.Format("{0}/api/v2/dispatcher?{2}={1}", ConstSettings.CloudDomain, !_isAnonymous ? CloudApi.Account.AuthToken : 2.ToString(), !_isAnonymous ? "token" : "api");
                var uri = string.Format("{0}/api/v2/dispatcher?api=2", ConstSettings.CloudDomain);
                var token = CloudApi.Account.AuthToken;
                if (!string.IsNullOrEmpty(token))
                    uri += $"&token={token}";
                return uri;
            }
        }
    }
}
