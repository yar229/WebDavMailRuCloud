using log4net.Repository.Hierarchy;
using YaR.MailRuCloud.Api.Base;

namespace YaR.MailRuCloud.Api
{
    public class CloudSettings
    {
        private static readonly log4net.ILog Logger = log4net.LogManager.GetLogger(typeof(CloudSettings));

        public ITwoFaHandler TwoFaHandler { get; set; }

        public Protocol Protocol;
    }
}