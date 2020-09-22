using System;
using System.Collections.Generic;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using YaR.Clouds.Base;

namespace YaR.Clouds.SpecialCommands.Commands
{
    /// <summary>
    /// Join random file from cloud. If you got it - you are biggest f@kn lucker of Universe!
    /// </summary>
    public class FishCommand : SpecialCommand
    {
        private static readonly log4net.ILog Logger = log4net.LogManager.GetLogger(typeof(FishCommand));

        public FishCommand(Cloud cloud, string path, IList<string> parames) : base(cloud, path, parames)
        {
        }

        protected override MinMax<int> MinMaxParamsCount { get; } = new MinMax<int>(0);

        private static readonly Random Random = new Random();

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
                //var res = await new CreateFileRequest(Cloud.CloudApi, target, strRandomHash, randomSize, ConflictResolver.Rename).MakeRequestAsync();
                var res = await Cloud.Account.RequestRepo.AddFile(target, strRandomHash, randomSize, DateTime.Now,  ConflictResolver.Rename);
                if (res.Success)
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
                string content = string.Empty;
                try
                {
                    using (WebClient client = new WebClient())
                    {
                        string htmlCode = client.DownloadString("http://www.smartphrase.com/cgi-bin/randomphrase.cgi?spanish&humorous&normal&15&2&12&16&1&5");
                        content = Regex.Match(htmlCode,
                                @"</FORM>\s*</TD>\s*<TD\s*ALIGN=""center""\s*WIDTH=\d+\s*BGCOLOR="".DCDCFF""\s*>\s*(?<phrase>.*?)<P>\s*(?<phraseeng>.*?)\s*<P>")
                            .Groups["phraseeng"].Value;
                    }
                }
                catch (Exception)
                {
                    // ignored
                }
                if (string.IsNullOrEmpty(content))
                    content = @"Maybe next time ¯\_(ツ)_/¯";

                Cloud.UploadFile(WebDavPath.Combine(Path, $"{DateTime.Now:yyyy-MM-dd hh-mm-ss} Not today, dude.txt"), content);
            }

            return SpecialCommandResult.Success;

        }
    }
}