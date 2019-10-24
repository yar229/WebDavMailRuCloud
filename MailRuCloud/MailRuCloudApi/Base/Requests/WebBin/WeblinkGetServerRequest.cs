namespace YaR.MailRuCloud.Api.Base.Requests.WebBin
{
    internal class WeblinkGetServerRequest : ServerRequest
    {
        public WeblinkGetServerRequest(HttpCommonSettings settings) : base(settings)
        {
        }

        protected override string RelationalUri => "https://dispatcher.cloud.mail.ru/G";
    }
}