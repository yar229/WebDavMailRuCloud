using System;

namespace YaR.Clouds.Base
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
            return WdmrcDots + (SplitInfo?.PartNumber.ToString("D3") ?? "000") + (CryptInfo?.AlignBytes.ToString("x") ?? string.Empty);
        }

        public string ToString(bool withName)
        {
            return withName
                ? CleanName + ToString()
                : ToString();
        }

        public static FilenameServiceInfo Parse(string filename)
        {
            

            static int HexToInt(char h)
            {
                return h switch
                {
                    >= '0' and <= '9' => h - '0',
                    >= 'a' and <= 'f' => h - 'a' + 10,
                    >= 'A' and <= 'F' => h - 'A' + 10,
                    _ => -1
                };
            }

            static bool IsDigit(char c) => c >= '0' && c <= '9';
            

            var res = new FilenameServiceInfo { CleanName = filename, SplitInfo = new FileSplitInfo { IsHeader = true } };

            if (filename.Length < 11)
                return res;

            var fns = filename.AsSpan();

            int pos = fns.LastIndexOf(WdmrcDots.AsSpan());
            if (pos < 0)
                return res;

            int startpos = pos;

            pos += WdmrcDots.Length;
            int parselen = fns.Length - pos;

            int align = parselen == 4 ? HexToInt(fns[pos + 3]) : -1;
            bool hasDigits = (parselen == 3 || (parselen == 4 && align > -1)) 
                             && IsDigit(fns[pos]) && IsDigit(fns[pos + 1]) && IsDigit(fns[pos + 2]);

            if (!hasDigits) 
                return res;

            res.CleanName = fns.Slice(0, startpos).ToString();
            
            res.SplitInfo.IsHeader = false;
            #if NET48
                res.SplitInfo.PartNumber = int.Parse(fns.Slice(pos, 3).ToString());
            #else
                res.SplitInfo.PartNumber = int.Parse(fns.Slice(pos, 3));
            #endif

            if (align > -1)
                res.CryptInfo = new CryptInfo { AlignBytes = (uint)align };

            return res;
        }

        private const string WdmrcDots = ".wdmrc.";
    }
}