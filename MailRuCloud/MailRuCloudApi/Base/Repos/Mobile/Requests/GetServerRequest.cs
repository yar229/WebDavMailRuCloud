using YaR.MailRuCloud.Api.Base.Requests;

namespace YaR.MailRuCloud.Api.Base.Repos.Mobile.Requests
{
    internal class GetServerRequest : ServerRequest
    {
        public GetServerRequest(HttpCommonSettings settings) : base(settings)
        {
        }
        
        protected override string RelationalUri => "https://dispatcher.cloud.mail.ru/d";
    }
}