using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace YaR.MailRuCloud.Api.SpecialCommands
{
    public class SharedFolderJoinCommand: SpecialCommand
    {
        public SharedFolderJoinCommand(MailRuCloud cloud, string path, IList<string> parames): base(cloud, path, parames)
        {
        }

        protected override MinMax<int> MinMaxParamsCount { get; } = new MinMax<int>(1);

        private string Value
        {
            get
            {
                var m = Regex.Match(Parames[0], @"(?snx-) (https://?cloud.mail.ru/public)?(?<data>/\w*/?\w*)/?\s*");

                return m.Success
                    ? m.Groups["data"].Value
                    : string.Empty;
            }
        }

        public override Task<SpecialCommandResult> Execute()
        {
            bool k = Cloud.CloneItem(Path, Value).Result;
            return Task.FromResult(new SpecialCommandResult{IsSuccess = k});
        }
    }
}