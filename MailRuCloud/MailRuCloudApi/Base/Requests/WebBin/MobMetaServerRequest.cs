using System;

namespace YaR.MailRuCloud.Api.Base.Requests.WebBin
{
    internal class MobMetaServerRequest : ServerRequest
    {
        public MobMetaServerRequest(HttpCommonSettings settings) : base(settings)
        {
        }

        protected override string RelationalUri => "https://dispatcher.cloud.mail.ru/m";
    }
}