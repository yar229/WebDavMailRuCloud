using System.Collections.Generic;
using System.Threading.Tasks;

namespace YaR.Clouds.SpecialCommands.Commands
{
    public class RemoveBadLinksCommand : SpecialCommand
    {
        public RemoveBadLinksCommand(Cloud cloud, string path, IList<string> parames): base(cloud, path, parames)
        {
        }

        protected override MinMax<int> MinMaxParamsCount { get; } = new(0);

        public override Task<SpecialCommandResult> Execute()
        {
            Cloud.RemoveDeadLinks();
            return Task.FromResult(SpecialCommandResult.Success);
        }
    }
}