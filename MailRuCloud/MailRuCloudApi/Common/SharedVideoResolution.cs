using System.Runtime.Serialization;

namespace YaR.MailRuCloud.Api.Common
{
    public enum SharedVideoResolution
    {
        [EnumMember(Value = "0p")]
        All,
        [EnumMember(Value = "240p")]
        R240,
        [EnumMember(Value = "360p")]
        R360,
        [EnumMember(Value = "480p")]
        R480,
        [EnumMember(Value = "720p")]
        R720,
        [EnumMember(Value = "1080p")]
        R1080
    }
}