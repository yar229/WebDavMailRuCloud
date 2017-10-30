using System.Threading.Tasks;


namespace YaR.MailRuCloud.Api.SpecialCommands
{
    public abstract class SpecialCommand
    {
        //TODO: remove link to NWebDav.Server
        public abstract Task<SpecialCommandResult> Execute();
    }
}
