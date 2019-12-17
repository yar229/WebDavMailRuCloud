using YaR.MailRuCloud.Api.Base.Requests;

namespace YaR.MailRuCloud.Api.Base.Repos.MailRuCloud.Mobile.Requests
{
    internal class WeblinkGetServerRequest : ServerRequest
    {
        public WeblinkGetServerRequest(HttpCommonSettings settings) : base(settings)
        {
        }

        protected override string RelationalUri => "https://dispatcher.cloud.mail.ru/G";
    }
}