using System;
using System.Collections.Generic;
using System.Text;
using YaR.MailRuCloud.Api.Base.Requests.Mobile;

namespace YaR.MailRuCloud.Api.Base.Requests.Types
{
    public class AddFileResult
    {
        public bool Success { get; set; }
        public string Path { get; set; }
    }
}
