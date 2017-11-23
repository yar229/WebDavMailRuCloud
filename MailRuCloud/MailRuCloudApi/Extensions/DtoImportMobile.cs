using YaR.MailRuCloud.Api.Base.Requests.Mobile;

namespace YaR.MailRuCloud.Api.Extensions
{
    internal static class DtoImportMobile
    {
        public static string ToToken(this MobAuthRequest.Result data)
        {
            var res = data.access_token;
            return res;
        }
    }
}