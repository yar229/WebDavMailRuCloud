using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace YaR.MailRuCloud.Api.Base
{
    public class SplittedFile : File
    {
        public SplittedFile(IList<File> files)
        {
            FileHeader = files.First(f => !Regex.Match(f.Name, @".wdmrc.\d\d\d\Z").Success);
            Files = files;
            Parts = files
                .Where(f => Regex.Match(f.Name, @".wdmrc.\d\d\d\Z").Success)
                .OrderBy(f => f.Name)
                .ToList();

            FullPath = FileHeader.FullPath;
        }


        public override FileSize Size => Parts.Sum(f => f.Size);

        public override string Hash => FileHeader.Hash;

        public override DateTime CreationTimeUtc => FileHeader.CreationTimeUtc;
        public override DateTime LastWriteTimeUtc => FileHeader.LastWriteTimeUtc;
        public override DateTime LastAccessTimeUtc => FileHeader.LastAccessTimeUtc;

        private File FileHeader { get; }

        /// <summary>
        /// List of phisical files contains data
        /// </summary>
        public override List<File> Parts { get; }

        public override IList<File> Files { get; }


        public override File New(string newfullPath)
        {
            string path = WebDavPath.Parent(newfullPath);

            var flist = Files
                .Select(f => f.New(WebDavPath.Combine(path, f.Name)))
                .ToList();
            var spfile = new SplittedFile(flist);
            return spfile;
        }
    }
}