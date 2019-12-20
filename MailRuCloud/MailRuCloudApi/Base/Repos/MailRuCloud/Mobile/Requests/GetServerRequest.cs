using YaR.Clouds.Base.Requests;

namespace YaR.Clouds.Base.Repos.MailRuCloud.Mobile.Requests
{
    internal class GetServerRequest : ServerRequest
    {
        public GetServerRequest(HttpCommonSettings settings) : base(settings)
        {
        }
        
        protected override string RelationalUri => "https://dispatcher.cloud.mail.ru/d";
    }
}