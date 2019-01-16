using System.Collections.Generic;
using System.Threading.Tasks;
using YaR.MailRuCloud.Api.Base;
using YaR.MailRuCloud.Api.Base.Requests.Types;

namespace YaR.MailRuCloud.Api.SpecialCommands
{
    public class ShareCommand : SpecialCommand
    {
        public ShareCommand(MailRuCloud cloud, string path, IList<string> parames) : base(cloud, path, parames)
        {
        }

        protected override MinMax<int> MinMaxParamsCount { get; } = new MinMax<int>(0, 1);

        public override async Task<SpecialCommandResult> Execute()
        {
            string path;
            string param = Parames.Count == 0 ? string.Empty : Parames[0].Replace("\\", WebDavPath.Separator);

            if (Parames.Count == 0)
                path = Path;
            else if (param.StartsWith(WebDavPath.Separator))
                path = param;
            else
                path = WebDavPath.Combine(Path, param);

            var entry = await Cloud.GetItemAsync(path);
            if (null == entry)
                return SpecialCommandResult.Fail;

            await Cloud.Publish(entry);
            return SpecialCommandResult.Success;
        }
    }


    public class ShareVCommand : SpecialCommand
    {
        public ShareVCommand(MailRuCloud cloud, string path, IList<string> parames) : base(cloud, path, parames)
        {
        }

        protected override MinMax<int> MinMaxParamsCount { get; } = new MinMax<int>(0, 1);

        public override async Task<SpecialCommandResult> Execute()
        {
            string path;
            string param = Parames.Count == 0 ? string.Empty : Parames[0].Replace("\\", WebDavPath.Separator);

            if (Parames.Count == 0)
                path = Path;
            else if (param.StartsWith(WebDavPath.Separator))
                path = param;
            else
                path = WebDavPath.Combine(Path, param);

            var entry = await Cloud.GetItemAsync(path);
            if (null == entry)
                return SpecialCommandResult.Fail;

            await Cloud.Publish(entry, true, true);
            return SpecialCommandResult.Success;
        }
    }
}