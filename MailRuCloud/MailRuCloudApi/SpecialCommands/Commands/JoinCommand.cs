using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using YaR.Clouds.Base;

namespace YaR.Clouds.SpecialCommands.Commands
{
    public class JoinCommand: SpecialCommand
    {
        public JoinCommand(Cloud cloud, string path, IList<string> parames): base(cloud, path, parames)
        {
            var m = Regex.Match(Parames[0], @"(?snx-) (https://?cloud.mail.ru/public)?(?<data>/\w*/?\w*)/?\s*");

            if (m.Success) //join by shared link
                _func = () => ExecuteByLink(Path, m.Groups["data"].Value);
            else 
            {
                var mhash = Regex.Match(Parames[0], @"#(?<data>\w+)");
                var msize = Regex.Match(Parames[1], @"(?<data>\w+)");
                if (mhash.Success && msize.Success && Parames.Count == 3) //join by hash and size
                {
                    _func = () => ExecuteByHash(Path, mhash.Groups["data"].Value, long.Parse(Parames[1]), Parames[2]);
                }
            }
        }

        protected override MinMax<int> MinMaxParamsCount { get; } = new MinMax<int>(1, 3);

        private readonly Func<Task<SpecialCommandResult>> _func;

        public override Task<SpecialCommandResult> Execute()
        {
            if (_func != null)
                return _func();

            return Task.FromResult(new SpecialCommandResult(false, "Invalid parameters"));
        }

        private async Task<SpecialCommandResult> ExecuteByLink(string path, string link)
        {
            var k = await Cloud.CloneItem(path, link);
            return new SpecialCommandResult(k.IsSuccess);
        }

        private async Task<SpecialCommandResult> ExecuteByHash(string path, string hash, long size, string paramPath)
        {
            string fpath = WebDavPath.IsFullPath(paramPath)
                ? paramPath
                : WebDavPath.Combine(path, paramPath);

            //TODO: now mail.ru only
            var k = await Cloud.AddFile(new FileHashMrc(hash), fpath, size, ConflictResolver.Rename);
            return new SpecialCommandResult(k.Success);
        }
    }
}