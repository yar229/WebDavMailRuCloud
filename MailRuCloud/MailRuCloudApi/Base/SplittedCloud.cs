using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace YaR.MailRuCloud.Api.Base
{
    public class SplittedCloud : MailRuCloud
    {
        public SplittedCloud(string login, string password, ITwoFaHandler twoFaHandler) : base(login, password, twoFaHandler)
        {
        }

        public override async Task<Entry> GetItems(string path)
        {
            Entry entry = await base.GetItems(path);

            if (null == entry) return null;

            var groupedFiles = entry.Files
                .GroupBy(f => Regex.Match(f.Name, @"(?<name>.*?)(\.wdmrc\.(crc|\d\d\d))?\Z").Groups["name"].Value, file => file)
                .Select(group => group.Count() == 1
                    ? group.First()
                    : new SplittedFile(group.ToList()))
                .ToList();

            var newEntry = new Entry(entry.Folders, groupedFiles, entry.FullPath) {Size = entry.Size};

            return newEntry;
        }
    }
}
