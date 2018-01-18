using System.Net;

namespace YaR.MailRuCloud.Api.Base.Requests.WebBin
{
    internal class GetServerRequest : ServerRequest
    {
        public GetServerRequest(HttpCommonSettings settings) : base(settings)
        {
        }
        
        protected override string RelationalUri => "https://dispatcher.cloud.mail.ru/d";
    }
}