using System.Collections.Generic;
using Newtonsoft.Json;

namespace YaR.Clouds.Base.Requests.Types
{
    internal class AccountInfoRequestResult : CommonOperationResult<AccountInfoRequestResult.AccountInfoBody>
    {
        public class AccountInfoBody
        {
            [JsonProperty("cloud")] public CloudInfo Cloud { get; set; }
            [JsonProperty("domain")] public string Domain { get; set; }
            [JsonProperty("ui")] public UIInfo UI { get; set; }
            [JsonProperty("newbie")] public bool Newbie { get; set; }
            [JsonProperty("account_type")] public string AccountType { get; set; }
            [JsonProperty("login")] public string Login { get; set; }

            public class UIInfo
            {
                [JsonProperty("sidebar")] public bool Sidebar { get; set; }
                [JsonProperty("sort")] public FolderInfoResult.FolderInfoBody.FolderInfoSort Sort { get; set; }
                [JsonProperty("kind")] public string Kind { get; set; }
                [JsonProperty("thumbs")] public bool Thumbs { get; set; }
                [JsonProperty("expand_loader")] public bool ExpandLoader { get; set; }
            }

            public class CloudInfo
            {
                [JsonProperty("enable")] public EnableInfo Enable { get; set; }
                [JsonProperty("beta")] public BetaInfo Beta { get; set; }
                [JsonProperty("bonuses")] public BonusesInfo Bonuses { get; set; }
                [JsonProperty("file_size_limit")] public long FileSizeLimit { get; set; }
                [JsonProperty("space")] public SpaceInfo Space { get; set; }
                [JsonProperty("billing")] public BillingInfo Billing { get; set; }

                public class EnableInfo
                {
                    [JsonProperty("sharing")] public bool Sharing { get; set; }
                }

                public class BetaInfo
                {
                    [JsonProperty("allowed")] public bool IsAllowed { get; set; }
                    [JsonProperty("asked")] public bool Asked { get; set; }
                }

                public class BonusesInfo
                {
                    [JsonProperty("camera_upload")] public bool CameraUpload { get; set; }
                    [JsonProperty("desktop")] public bool Desktop { get; set; }
                    [JsonProperty("mobile")] public bool Mobile { get; set; }
                    [JsonProperty("complete")] public bool Complete { get; set; }
                    [JsonProperty("registration")] public bool Registration { get; set; }
                    [JsonProperty("feedback")] public bool Feedback { get; set; }
                    [JsonProperty("links")] public bool Links { get; set; }
                }

                public class SpaceInfo
                {
                    [JsonProperty("bytes_total")] public long BytesTotal { get; set; }
                    [JsonProperty("overquota")] public bool IsOverquota { get; set; }
                    [JsonProperty("bytes_used")] public long BytesUsed { get; set; }
                }

                public class BillingInfo
                {
                    [JsonProperty("active_cost_id")] public string ActiveCostID { get; set; }
                    [JsonProperty("active_rate_id")] public string ActiveRateID { get; set; }
                    [JsonProperty("auto_prolong")] public bool AutoProlong { get; set; }
                    [JsonProperty("subscription")] public List<object> Subscription { get; set; }
                    [JsonProperty("prolong")] public bool Prolong { get; set; }
                    [JsonProperty("enabled")] public bool Enabled { get; set; }
                    [JsonProperty("expires")] public int Expires { get; set; }
                }
            }
        }
    }
}