using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YaR.Clouds.Base;
using YaR.Clouds.Base.Repos;

namespace YaR.Clouds.SpecialCommands.Commands
{
    public class ListCommand : SpecialCommand
    {
        //private static readonly log4net.ILog Logger = log4net.LogManager.GetLogger(typeof(FishCommand));

        public ListCommand(Cloud cloud, string path, IList<string> parames) : base(cloud, path, parames)
        {
        }

        protected override MinMax<int> MinMaxParamsCount { get; } = new MinMax<int>(0, 1);

        public override async Task<SpecialCommandResult> Execute()
        {
            string name = Parames.Count > 0 && !string.IsNullOrWhiteSpace(Parames[0])
                ? Parames[0]
                : ".wdmrc.list.lst";
            string target = WebDavPath.Combine(Path, name);

            var data = await Cloud.Account.RequestRepo.FolderInfo(RemotePath.Get(Path));

            var sb = new StringBuilder();
            foreach (var e in Flat(data))
            {
                string hash = (e as File)?.Hash ?? "-";
                string link = e.PublicLinks.Any() ? e.PublicLinks.First().Uri.OriginalString : "-";
                sb.AppendLine(
                    $"{e.FullPath}\t{e.Size.DefaultValue}\t{e.CreationTimeUtc:yyyy.MM.dd HH:mm:ss}\t{hash}\t{link}");
            }

            Cloud.UploadFile(target, sb.ToString());

            return SpecialCommandResult.Success;
        }

        public IEnumerable<IEntry> Flat(IEntry entry)
        {
            yield return entry;

            if (entry is Folder folder)
            {
                var ifolders = folder.Entries
                    .AsParallel()
                    .WithDegreeOfParallelism(5)
                    .Select(it => it is File
                        ? it
                        : it is Folder ifolder
                            ? ifolder.IsChildsLoaded
                                ? ifolder
                                : Cloud.Account.RequestRepo.FolderInfo(RemotePath.Get(it.FullPath), depth: 3).Result
                            : throw new NotImplementedException("Unknown item type"))
                    .OrderBy(it => it.Name);
                    
                foreach (var item in ifolders.SelectMany(Flat))
                    yield return item;
            }
        }
    }
}