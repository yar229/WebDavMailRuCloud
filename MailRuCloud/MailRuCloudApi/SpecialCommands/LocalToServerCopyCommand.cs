using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using YaR.MailRuCloud.Api.Base;

namespace YaR.MailRuCloud.Api.SpecialCommands
{
    public class LocalToServerCopyCommand : SpecialCommand
    {
        private static readonly log4net.ILog Logger = log4net.LogManager.GetLogger(typeof(Account));

        public LocalToServerCopyCommand(MailRuCloud cloud, string path, IList<string> parames) : base(cloud, path, parames)
        {
        }

        protected override MinMax<int> MinMaxParamsCount { get; } = new MinMax<int>(1);

        public override async Task<SpecialCommandResult> Execute()
        {
            var res = await Task.Run(async () =>
            {
                var sourceFileInfo = new FileInfo(Parames[0]);

                string name = sourceFileInfo.Name;
                string targetPath = WebDavPath.Combine(Path, name);

                Logger.Info($"COMMAND:COPY:{Parames[0]}");

                using (var source = System.IO.File.Open(Parames[0], FileMode.Open, FileAccess.Read, FileShare.Read))
                using (var target = await Cloud.GetFileUploadStream(targetPath, sourceFileInfo.Length).ConfigureAwait(false))
                {
                    source.CopyTo(target);
                }

                return SpecialCommandResult.Success;
            });

            return res;
        }
    }
}