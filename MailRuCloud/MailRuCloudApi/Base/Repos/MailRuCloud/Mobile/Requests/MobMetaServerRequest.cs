using YaR.MailRuCloud.Api.Base.Requests;

namespace YaR.MailRuCloud.Api.Base.Repos.MailRuCloud.Mobile.Requests
{
    internal class MobMetaServerRequest : ServerRequest
    {
        public MobMetaServerRequest(HttpCommonSettings settings) : base(settings)
        {
        }

        protected override string RelationalUri => "https://dispatcher.cloud.mail.ru/m";
    }
}