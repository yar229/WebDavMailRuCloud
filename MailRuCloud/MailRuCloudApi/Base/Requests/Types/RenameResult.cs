using System;
using System.Collections.Generic;
using System.Text;

namespace YaR.MailRuCloud.Api.Base.Requests.Types
{
    public class RenameResult
    {
        public string email { get; set; }
        public string body { get; set; }
        public long time { get; set; }
        public int status { get; set; }
    }
}
