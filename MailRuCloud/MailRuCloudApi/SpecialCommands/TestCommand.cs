using System.Collections.Generic;
using System.Threading.Tasks;
using YaR.MailRuCloud.Api.Base;

namespace YaR.MailRuCloud.Api.SpecialCommands
{
    public class TestCommand : SpecialCommand
    {
        public TestCommand(MailRuCloud cloud, string path, IList<string> parames) : base(cloud, path, parames)
        {
        }

        protected override MinMax<int> MinMaxParamsCount { get; } = new MinMax<int>(1);

        public override async Task<SpecialCommandResult> Execute()
        {
            string path = Parames[0].Replace("\\", WebDavPath.Separator);

            if (!(await Cloud.GetItemAsync(path) is File entry))
                return SpecialCommandResult.Fail;

            //var auth = await new OAuthRequest(Cloud.CloudApi).MakeRequestAsync();

            bool removed = await Cloud.Remove(entry, false);
            if (removed)
            {
                //var addreq = await new MobAddFileRequest(Cloud.CloudApi, entry.FullPath, entry.Hash, entry.Size, new DateTime(2010, 1, 1), ConflictResolver.Rename)
                //    .MakeRequestAsync();
            }

            return SpecialCommandResult.Success;
        }
    }

}