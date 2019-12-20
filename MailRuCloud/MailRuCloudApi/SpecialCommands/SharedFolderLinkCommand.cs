using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using YaR.Clouds.Base.Repos;
using YaR.Clouds.Base.Repos.MailRuCloud;

namespace YaR.Clouds.SpecialCommands
{
    public class SharedFolderLinkCommand : SpecialCommand
    {
        public SharedFolderLinkCommand(Cloud cloud, string path, IList<string> parames): base(cloud, path, parames)
        {
        }

        protected override MinMax<int> MinMaxParamsCount { get; } = new MinMax<int>(1, 2);

        public override async Task<SpecialCommandResult> Execute()
        {
            var m = Regex.Match(Parames[0], @"(?snx-)\s* (https://?cloud.mail.ru/public)?(?<url>.*)/? \s*");

            if (!m.Success) return SpecialCommandResult.Fail;

            //TODO: make method in MailRuCloud to get entry by url
            //var item = await new ItemInfoRequest(Cloud.CloudApi, m.Groups["url"].Value, true).MakeRequestAsync();
            var item = await Cloud.Account.RequestRepo.ItemInfo(m.Groups["url"].Value, true);
            var entry = item.ToEntry(Cloud.Repo.PublicBaseUrlDefault);
            if (null == entry)
                return SpecialCommandResult.Fail;

            string name = Parames.Count > 1 && !string.IsNullOrWhiteSpace(Parames[1])
                    ? Parames[1]
                    : entry.Name;

            var res = await Cloud.LinkItem(new Uri(Cloud.Repo.PublicBaseUrlDefault + m.Groups["url"].Value, UriKind.Absolute),  //m.Groups["url"].Value, 
                Path, name, entry.IsFile, entry.Size, entry.CreationTimeUtc);

            return new SpecialCommandResult(res);
        }
    }
}