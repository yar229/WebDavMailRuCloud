using System.Threading.Tasks;
using NWebDav.Server.Stores;

namespace YaR.MailRuCloud.Api.SpecialCommands
{
    public abstract class SpecialCommand
    {
        //TODO: remove link to NWebDav.Server
        public abstract Task<StoreCollectionResult> Execute();
    }
}
