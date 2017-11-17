using System;
using System.Collections.Generic;
using System.Linq;

namespace YaR.MailRuCloud.Api.Base
{
    public class SplittedFile : File
    {
        public SplittedFile(IList<File> files)
        {
            FileHeader = files.First(f => !f.ServiceInfo.SplitInfo.IsPart);
            Files = files;
            Parts = files
                .Where(f => f.ServiceInfo.SplitInfo.IsPart)
                .OrderBy(f => f.ServiceInfo.SplitInfo.PartNumber)
                .ToList();

            FullPath = FileHeader.FullPath;

            ServiceInfo = new FilenameServiceInfo
            {
                CleanName = FileHeader.Name,
                CryptInfo = files.First(f => f.ServiceInfo.SplitInfo.IsPart).ServiceInfo.CryptInfo,
                SplitInfo = new FileSplitInfo
                {
                    IsHeader = true
                }
            };
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