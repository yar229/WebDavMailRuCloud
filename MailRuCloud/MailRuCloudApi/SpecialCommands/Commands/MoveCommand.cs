using System.Collections.Generic;
using System.Threading.Tasks;
using YaR.Clouds.Base;

namespace YaR.Clouds.SpecialCommands.Commands
{
    public class MoveCommand : SpecialCommand
    {
        public MoveCommand(Cloud cloud, string path, IList<string> parames) : base(cloud, path, parames)
        {
        }

        protected override MinMax<int> MinMaxParamsCount { get; } = new(1, 2);

        public override async Task<SpecialCommandResult> Execute()
        {
            string source = WebDavPath.Clean(Parames.Count == 1 ? Path : Parames[0]);
            string target = WebDavPath.Clean(Parames.Count == 1 ? Parames[0] : Parames[1]);

            var sourceEntry = await Cloud.GetItemAsync(source);
            if (null == sourceEntry)
                return SpecialCommandResult.Fail;

            var res = await Cloud.MoveAsync(sourceEntry, target);
            return new SpecialCommandResult(res);
        }
    }
}