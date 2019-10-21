using System;
using System.Text.RegularExpressions;

namespace YaR.MailRuCloud.Api.Base
{
    public class FilenameServiceInfo
    {
        public string CleanName { get; set; }

        public bool IsCrypted => CryptInfo != null;
        public CryptInfo CryptInfo { get; set; }

        //public bool IsSplitted => SplitInfo != null;
        public FileSplitInfo SplitInfo { get; set; }

        public override string ToString()
        {
            return ".wdmrc." + (SplitInfo?.PartNumber.ToString("D3") ?? "000") + (CryptInfo?.AlignBytes.ToString("x") ?? string.Empty);
        }

        public string ToString(bool withName)
        {
            return withName
                ? CleanName + ToString()
                : ToString();
        }

        public static FilenameServiceInfo Parse(string filename)
        {
            var res = new FilenameServiceInfo();

            var m = Regex.Match(filename, @"\A(?<cleanname>.*?)(\.wdmrc\.(?<partnumber>\d\d\d)(?<align>[0-9a-f])?)?\Z", RegexOptions.Compiled);
            if (!m.Success)
                throw new InvalidOperationException("Cannot parse filename");

            res.CleanName = m.Groups["cleanname"].Value;

            string partnumber = m.Groups["partnumber"].Value;
            res.SplitInfo = new FileSplitInfo
            {
                IsHeader = string.IsNullOrEmpty(partnumber),
                PartNumber = string.IsNullOrEmpty(partnumber) ? 0 : int.Parse(m.Groups["partnumber"].Value)
            };

            string align = m.Groups["align"].Value;
            if (!string.IsNullOrEmpty(align))
            {
                res.CryptInfo = new CryptInfo
                {
                    AlignBytes = Convert.ToUInt32(align, 16)
                };
            }

            return res;
        }
    }
}