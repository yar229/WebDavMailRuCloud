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

        public static MailRuCloud.PathResult ToPathResult(this CreateFolderRequest.Result data)
        {
            var res = new MailRuCloud.PathResult
            {
                IsSuccess = data.OperationResult == OperationResult.Ok,
                Path = data.Path
            };
            return res;
        }
    }
}