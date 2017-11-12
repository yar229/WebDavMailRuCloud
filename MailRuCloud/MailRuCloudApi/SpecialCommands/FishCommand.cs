using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using YaR.MailRuCloud.Api.Base;
using YaR.MailRuCloud.Api.Base.Requests;
using YaR.MailRuCloud.Api.Base.Requests.Types;

namespace YaR.MailRuCloud.Api.SpecialCommands
{
    /// <summary>
    /// Join random file from cloud. If you got it - you are biggest f@kn lucker of Universe!
    /// </summary>
    public class FishCommand : SpecialCommand
    {
        private static readonly log4net.ILog Logger = log4net.LogManager.GetLogger(typeof(FishCommand));

        public FishCommand(MailRuCloud cloud, string path, IList<string> parames) : base(cloud, path, parames)
        {
        }

        protected override MinMax<int> MinMaxParamsCount { get; } = new MinMax<int>(0);

        private static Random Random = new Random();

        public override async Task<SpecialCommandResult> Execute()
        {
            string name = "FISHA.YEA";
            string target = WebDavPath.Combine(Path, name);

            var randomHash = new byte[20];
            Random.NextBytes(randomHash);
            string strRandomHash = BitConverter.ToString(randomHash).Replace("-", string.Empty);

            long randomSize = Random.Next(21, int.MaxValue);

            try
            {
                var res = await new CreateFileRequest(Cloud.CloudApi, target, strRandomHash, randomSize, ConflictResolver.Rename)
                    .MakeRequestAsync();
                if (res.status == 200)
                {
                    Logger.Warn("╔╗╔╗╔╦══╦╗╔╗╔╗╔╦╦╗");
                    Logger.Warn("║║║║║║╔╗║║║║║║║║║║");
                    Logger.Warn("║║║║║║║║║║║║║║║║║║");
                    Logger.Warn("║║║║║║║║║║║║║║╚╩╩╝");
                    Logger.Warn("║╚╝╚╝║╚╝║╚╝╚╝║╔╦╦╗");
                    Logger.Warn("╚═╝╚═╩══╩═╝╚═╝╚╩╩╝");
                    Logger.Warn("");
                    Logger.Warn("¦̵̱ ̵̱ ̵̱ ̵̱ ̵̱(̢ ̡͇̅└͇̅┘͇̅ (▤8כ−◦");
                    
                }
            }
            catch (Exception)
            {
                Cloud.UploadFile(WebDavPath.Combine(Path, $"{DateTime.Now:yyyy-MM-dd hh-mm-ss} Not today, dude.txt"), @"Maybe next time ¯\_(ツ)_/¯");
            }

            return SpecialCommandResult.Success;

        }
    }
}