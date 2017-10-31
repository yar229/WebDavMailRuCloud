using System.Threading.Tasks;


namespace YaR.MailRuCloud.Api.SpecialCommands
{
    public abstract class SpecialCommand
    {
        public abstract Task<SpecialCommandResult> Execute();
    }
}
