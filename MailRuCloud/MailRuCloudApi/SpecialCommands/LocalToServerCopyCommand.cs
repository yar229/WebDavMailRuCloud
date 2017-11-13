using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using YaR.MailRuCloud.Api.Base;

namespace YaR.MailRuCloud.Api.SpecialCommands
{
    public class LocalToServerCopyCommand : SpecialCommand
    {
        public LocalToServerCopyCommand(MailRuCloud cloud, string path, IList<string> parames) : base(cloud, path, parames)
        {
        }

        protected override MinMax<int> MinMaxParamsCount { get; } = new MinMax<int>(1);

        public override async Task<SpecialCommandResult> Execute()
        {
            var res = await Task.Run(() =>
            {
                var sourceFileInfo = new FileInfo(Parames[0]);

                string name = sourceFileInfo.Name;
                string targetPath = WebDavPath.Combine(Path, name);

                using (var source = System.IO.File.Open(Parames[0], FileMode.Open, FileAccess.Read, FileShare.Read))
                using (var target = Cloud.GetFileUploadStream(targetPath, sourceFileInfo.Length))
                {
                    source.CopyTo(target);
                }

                return SpecialCommandResult.Success;
            });

            return res;
        }
    }
}