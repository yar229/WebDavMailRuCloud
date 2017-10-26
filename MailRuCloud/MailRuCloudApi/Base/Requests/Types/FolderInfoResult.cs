// ReSharper disable All

using System.Collections.Generic;

namespace YaR.MailRuCloud.Api.Base.Requests.Types
{

    public class FolderInfoResult
    {
        public string email { get; set; }
        public FolderInfoBody body { get; set; }
        public long time { get; set; }
        public int status { get; set; }
    }


    public class FolderInfoCount
    {
        public int folders { get; set; }
        public int files { get; set; }
    }

    public class FolderInfoSort
    {
        public string order { get; set; }
        public string type { get; set; }
    }

    public class FolderInfoProps
    {
        public long mtime;
        public FolderInfoCount count { get; set; }
        public string tree { get; set; }
        public string name { get; set; }
        public int grev { get; set; }
        public long size { get; set; }
        public string kind { get; set; }
        public int rev { get; set; }
        public string type { get; set; }
        public string home { get; set; }
        public string weblink { get; set; }
        public string hash { get; set; }

    }

    public class FolderInfoBody
    {
        public FolderInfoCount count { get; set; }
        public string tree { get; set; }
        public string name { get; set; }
        public int grev { get; set; }
        public long size { get; set; }
        public FolderInfoSort sort { get; set; }
        public string kind { get; set; }
        public int rev { get; set; }
        public string type { get; set; }
        public string home { get; set; }
        public List<FolderInfoProps> list { get; set; }
    }

}
