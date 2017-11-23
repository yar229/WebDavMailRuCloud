using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using YaR.MailRuCloud.Api.Base;
using YaR.MailRuCloud.Api.Base.Requests.Mobile;

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

            if (!(await Cloud.GetItem(path) is File entry))
                return SpecialCommandResult.Fail;

            var auth = await new MobAuthRequest(Cloud.CloudApi)
                .MakeRequestAsync();

            await Cloud.Remove(entry, false);

            var addreq = await new MobAddFileRequest(Cloud.CloudApi, auth.access_token, entry.FullPath, StringToByteArray(entry.Hash), entry.Size, new DateTime(2010, 1, 1))
                .MakeRequestAsync();

            return SpecialCommandResult.Success;
        }


        private static byte[] StringToByteArray(String hex)
        {
            int len = hex.Length;
            byte[] bytes = new byte[len / 2];
            for (int i = 0; i < len; i += 2)
                bytes[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);
            return bytes;
        }
    }

}