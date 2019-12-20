using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using YaR.Clouds.Base;
using YaR.Clouds.Common;
using YaR.Clouds.Extensions;

namespace YaR.Clouds.SpecialCommands
{
    public class ShareCommand : SpecialCommand
    {

        public ShareCommand(Cloud cloud, string path, bool generateDirectVideoLink, bool makeM3UFile, IList<string> parames) : base(cloud, path, parames)
        {
            _generateDirectVideoLink = generateDirectVideoLink;
            _makeM3UFile = makeM3UFile;
        }


        private readonly bool _generateDirectVideoLink;
        private readonly bool _makeM3UFile;

        protected override MinMax<int> MinMaxParamsCount { get; } = new MinMax<int>(0, 2);

        public override async Task<SpecialCommandResult> Execute()
        {
            string path;
            string param = Parames.Count == 0 
                ? string.Empty 
                : Parames[0].Replace("\\", WebDavPath.Separator);
            SharedVideoResolution videoResolution = Parames.Count < 2 
                ? Cloud.Settings.DefaultSharedVideoResolution
                : EnumExtensions.ParseEnumMemberValue<SharedVideoResolution>(Parames[1]);

            if (Parames.Count == 0)
                path = Path;
            else if (param.StartsWith(WebDavPath.Separator))
                path = param;
            else
                path = WebDavPath.Combine(Path, param);

            var entry = await Cloud.GetItemAsync(path);
            if (null == entry)
                return SpecialCommandResult.Fail;

            try
            {
                await Cloud.Publish(entry, true, _generateDirectVideoLink, _makeM3UFile, videoResolution);
            }
            catch (Exception e)
            {
                return new SpecialCommandResult(false, e.Message);
            }
            
            return SpecialCommandResult.Success;
        }
    }
}