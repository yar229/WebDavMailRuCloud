using System;
using System.Collections.Generic;
using System.Linq;

namespace YaR.Clouds.Base
{
    public class SplittedFile : File
    {
        public SplittedFile(IList<File> files)
        {
            _fileHeader = files.First(f => !f.ServiceInfo.SplitInfo.IsPart);
            Files = files;
            Parts = files
                .Where(f => f.ServiceInfo.SplitInfo.IsPart)
                .OrderBy(f => f.ServiceInfo.SplitInfo.PartNumber)
                .ToList();

            FullPath = _fileHeader.FullPath;


            var cryptofile = files.FirstOrDefault(f => f.ServiceInfo.SplitInfo.IsPart && f.ServiceInfo.CryptInfo != null);
            ServiceInfo = new FilenameServiceInfo
            {
                CleanName = _fileHeader.Name,
                CryptInfo = cryptofile?.ServiceInfo?.CryptInfo,
                SplitInfo = new FileSplitInfo
                {
                    IsHeader = true
                }
            };
        }


        public override FileSize Size => Parts.Sum(f => f.Size);

        public override FileSize OriginalSize => Parts.Sum(f => f.OriginalSize);

        public override string Hash => FileHeader.Hash;

        public override DateTime CreationTimeUtc => FileHeader.CreationTimeUtc;
        public override DateTime LastWriteTimeUtc => FileHeader.LastWriteTimeUtc;
        public override DateTime LastAccessTimeUtc => FileHeader.LastAccessTimeUtc;

        protected override File FileHeader => _fileHeader;
        private readonly File _fileHeader;

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