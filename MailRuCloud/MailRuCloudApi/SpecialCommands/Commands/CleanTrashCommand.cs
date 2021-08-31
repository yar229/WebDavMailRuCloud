using System.Collections.Generic;
using System.Threading.Tasks;

namespace YaR.Clouds.SpecialCommands.Commands
{
    /// <summary>
    /// Очистка корзины
    /// </summary>
    public class CleanTrashCommand : SpecialCommand
    {
        public CleanTrashCommand(Cloud cloud, string path, IList<string> parames) : base(cloud, path, parames)
        {
        }

        protected override MinMax<int> MinMaxParamsCount { get; } = new(0);

        public override async Task<SpecialCommandResult> Execute()
        {
            Cloud.CleanTrash();

            return await Task.FromResult(SpecialCommandResult.Success);
        }
    }
}