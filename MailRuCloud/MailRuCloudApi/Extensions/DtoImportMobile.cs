using YaR.MailRuCloud.Api.Base.Requests.Mobile;
using YaR.MailRuCloud.Api.Base.Requests.Types;

namespace YaR.MailRuCloud.Api.Extensions
{
    internal static class DtoImportMobile
    {
        public static AddFileResult ToAddFileResult(this MobAddFileRequest.Result data)
        {
            var res = new AddFileResult
            {
                Success = data.OperationResult == OperationResult.Ok,
                Path = data.Path
            };
            return res;
        }



        public static string ToToken(this MobAuthRequest.Result data)
        {
            var res = data.access_token;
            return res;
        }

        public static CreateFolderResult ToCreateFolderResult(this CreateFolderRequest.Result data)
        {
            var res = new CreateFolderResult
            {
                IsSuccess = data.OperationResult == OperationResult.Ok,
                Path = data.Path
            };
            return res;
        }
    }
}