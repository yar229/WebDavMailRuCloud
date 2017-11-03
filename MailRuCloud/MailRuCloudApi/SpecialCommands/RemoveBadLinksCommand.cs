using System.Threading.Tasks;
using YaR.MailRuCloud.Api.Base;

namespace YaR.MailRuCloud.Api.SpecialCommands
{
    public class RemoveBadLinksCommand : SpecialCommand
    {
        private readonly MailRuCloud _cloud;

        public RemoveBadLinksCommand(MailRuCloud cloud)
        {
            _cloud = cloud;
        }

        public override Task<SpecialCommandResult> Execute()
        {
            _cloud.RemoveDeadLinks();
            return Task.FromResult(SpecialCommandResult.Success);
        }
    }
}