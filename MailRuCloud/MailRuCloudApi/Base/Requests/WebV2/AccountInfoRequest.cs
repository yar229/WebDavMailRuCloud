using System.Collections.Generic;
using System.Net;
using YaR.MailRuCloud.Api.Base.Requests.Repo;
using YaR.MailRuCloud.Api.Base.Requests.Types;

namespace YaR.MailRuCloud.Api.Base.Requests.WebV2
{
    class AccountInfoRequest : BaseRequestJson<AccountInfoRequest.Result>
    {
        public AccountInfoRequest(IWebProxy proxy, IAuth auth) : base(proxy, auth)
        {
        }

        protected override string RelationalUri => $"{ConstSettings.CloudDomain}/api/v2/user?token={Auth.AccessToken}";


        public class Result
        {
            public string email { get; set; }
            public AccountInfoBody body { get; set; }
            public long time { get; set; }
            public int status { get; set; }

            public class AccountInfoBody
            {
                public Cloud cloud { get; set; }
                public string domain { get; set; }
                public Ui ui { get; set; }
                public bool newbie { get; set; }
                public string account_type { get; set; }
                public string login { get; set; }

                public class Ui
                {
                    public bool sidebar { get; set; }
                    public FolderInfoResult.FolderInfoBody.FolderInfoSort sort { get; set; }
                    public string kind { get; set; }
                    public bool thumbs { get; set; }
                    public bool expand_loader { get; set; }
                }

                public class Cloud
                {
                    public Enable enable { get; set; }
                    public int metad { get; set; }
                    public Beta beta { get; set; }
                    public Bonuses bonuses { get; set; }
                    public long file_size_limit { get; set; }
                    public Space space { get; set; }
                    public Billing billing { get; set; }

                    public class Enable
                    {
                        public bool sharing { get; set; }
                    }

                    public class Beta
                    {
                        public bool allowed { get; set; }
                        public bool asked { get; set; }
                    }

                    public class Bonuses
                    {
                        public bool camera_upload { get; set; }
                        public bool desktop { get; set; }
                        public bool mobile { get; set; }
                        public bool complete { get; set; }
                        public bool registration { get; set; }
                        public bool feedback { get; set; }
                        public bool links { get; set; }
                    }

                    public class Space
                    {
                        public bool overquota { get; set; }
                        public long used { get; set; }
                        public long total { get; set; }
                    }

                    public class Billing
                    {
                        public string active_cost_id { get; set; }
                        public string active_rate_id { get; set; }
                        public bool auto_prolong { get; set; }
                        public List<object> subscription { get; set; }
                        public bool prolong { get; set; }
                        public bool enabled { get; set; }
                        public int expires { get; set; }
                    }
                }
            }
        }
    }
}
