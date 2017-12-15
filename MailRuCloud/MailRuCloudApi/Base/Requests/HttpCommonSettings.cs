using System.Net;

namespace YaR.MailRuCloud.Api.Base.Requests
{
    internal class HttpCommonSettings
    {
        public IWebProxy Proxy { get; set; }
        public string ClientId { get; set; }
        public string UserAgent { get; set; }
    }
}