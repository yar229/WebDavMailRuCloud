using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using YaR.MailRuCloud.Api.Base.Requests;
using YaR.MailRuCloud.Api.Extensions;

namespace YaR.MailRuCloud.Api.SpecialCommands
{
    public class SharedFolderLinkCommand : SpecialCommand
    {
        public SharedFolderLinkCommand(MailRuCloud cloud, string path, IList<string> parames): base(cloud, path, parames)
        {
        }

        protected override MinMax<int> MinMaxParamsCount { get; } = new MinMax<int>(1, 2);

        public override async Task<SpecialCommandResult> Execute()
        {
            var m = Regex.Match(Parames[0], @"(?snx-)\s* (https://?cloud.mail.ru/public)?(?<url>/\w*/\w*)/? \s*");

            if (!m.Success) return SpecialCommandResult.Fail;

            //TODO: make method in MailRuCloud to get entry by url
            var item = await new ItemInfoRequest(Cloud.CloudApi, m.Groups["url"].Value, true).MakeRequestAsync();
            var entry = item.ToEntry();
            if (null == entry)
                return SpecialCommandResult.Fail;

            string name = Parames.Count > 1 && !string.IsNullOrWhiteSpace(Parames[1])
                    ? Parames[1]
                    : entry.Name;

            Cloud.LinkItem(m.Groups["url"].Value, Path, name, item.IsFile, entry.Size, entry.CreationTimeUtc);

            return SpecialCommandResult.Success;
        }
    }
}