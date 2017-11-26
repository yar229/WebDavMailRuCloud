using System;
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


        public static AuthTokenResult ToAuthTokenResult(this MobAuthRequest.Result data)
        {
            var res = new AuthTokenResult
            {
                IsSuccess = true,
                Token = data.access_token,
                ExpiresIn = TimeSpan.FromSeconds(data.expires_in),
                RefreshToken = data.refresh_token
            };

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