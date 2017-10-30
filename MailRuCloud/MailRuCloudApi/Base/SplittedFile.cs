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
            _fileParts.AddRange(files
                .Where(f => Regex.Match(f.Name, @".wdmrc.\d\d\d\Z").Success)
                .OrderBy(f => f.Name));

            FullPath = FileHeader.FullPath; //FullPath = WebDavPath.Combine(FileHeader.Path, FileHeader.Name.Substring(0, FileHeader.Name.Length - HeaderSuffix.Length));
        }


        public override FileSize Size => _fileParts.Sum(f => f.Size);

        public override string Hash => FileHeader.Hash;

        public override DateTime CreationTimeUtc => FileHeader.CreationTimeUtc;
        public override DateTime LastWriteTimeUtc => FileHeader.LastWriteTimeUtc;
        public override DateTime LastAccessTimeUtc => FileHeader.LastAccessTimeUtc;

        /// <summary>
        /// List of physical files
        /// </summary>
        public override List<File> Parts => _fileParts;

        private File FileHeader { get; }
        private readonly List<File> _fileParts = new List<File>();
    }
}