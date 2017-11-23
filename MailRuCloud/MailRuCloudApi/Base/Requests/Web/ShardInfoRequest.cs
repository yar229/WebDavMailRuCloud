using YaR.MailRuCloud.Api.Base.Requests.Types;

namespace YaR.MailRuCloud.Api.Base.Requests.Web
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
                var uri = string.Format("{0}/api/v2/dispatcher?api=2", ConstSettings.CloudDomain);
                var token = CloudApi.Account.AuthToken.Value;
                if (!string.IsNullOrEmpty(token))
                    uri += $"&token={token}";
                return uri;
            }
        }
    }
}
