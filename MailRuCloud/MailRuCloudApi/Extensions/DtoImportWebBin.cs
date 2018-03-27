using System;
using System.Linq;
using YaR.MailRuCloud.Api.Base;
using YaR.MailRuCloud.Api.Base.Requests.Types;
using YaR.MailRuCloud.Api.Base.Requests.WebBin;
using YaR.MailRuCloud.Api.Base.Requests.WebBin.Types;

namespace YaR.MailRuCloud.Api.Extensions
{
    internal static class DtoImportWebBin
    {
	    public static IEntry ToEntry(this ListRequest.Result data)
	    {
			MailRuCloud.ItemType itemType = data.Item is FsFile ? MailRuCloud.ItemType.File : MailRuCloud.ItemType.Folder;

		    var entry = itemType == MailRuCloud.ItemType.File
			    ? (IEntry)data.ToFile()
			    : data.ToFolder();

		    return entry;
		}

	    public static YaR.MailRuCloud.Api.Base.File ToFile(this ListRequest.Result data)
	    {
		    var source = data.Item as FsFile;
		    var res = new File(data.FullPath, (long)source.Size, source.Sha1.ToHexString());
		    return res;
	    }

	    public static YaR.MailRuCloud.Api.Base.Folder ToFolder(this ListRequest.Result data)
	    {
		    var source = data.Item as FsFolder;
		    var res = new Folder((long)source.Size, data.FullPath);
		    foreach (var it in source.Items.OfType<FsFile>())
		    {
			    res.Files.Add(new File(it.FullPath, (long)it.Size, it.Sha1.ToHexString()));
		    }
		    foreach (var it in source.Items.OfType<FsFolder>())
		    {
			    res.Folders.Add(new Folder((long)it.Size, it.FullPath));
		    }

			return res;
		}





		public static CopyResult ToCopyResult(this MoveRequest.Result data, string newName)
        {
            var res = new CopyResult
            {
                IsSuccess = true,
                NewName = newName
            };
            return res;
        }

        public static RenameResult ToRenameResult(this MoveRequest.Result data)
        {
            var res = new RenameResult
            {
                IsSuccess = true
            };
            return res;
        }

        public static AddFileResult ToAddFileResult(this MobAddFileRequest.Result data)
        {
            var res = new AddFileResult
            {
                Success = data.IsSuccess,
                Path = data.Path
            };
            return res;
        }


        public static AuthTokenResult ToAuthTokenResult(this OAuthRefreshRequest.Result data, string refreshToken)
        {
            if (data.error_code > 0)
                throw new Exception($"OAuth: Error Code={data.error_code}, Value={data.error}, Description={data.error_description}");

            var res = new AuthTokenResult
            {
                IsSuccess = true,
                Token = data.access_token,
                ExpiresIn = TimeSpan.FromSeconds(data.expires_in),
                RefreshToken = refreshToken
            };

            return res;
        }

        public static AuthTokenResult ToAuthTokenResult(this OAuthRequest.Result data)
        {
            if (data.error_code > 0 && data.error_code != 15)
                throw new Exception($"OAuth: Error Code={data.error_code}, Value={data.error}, Description={data.error_description}");

            var res = new AuthTokenResult
            {
                IsSuccess = true,
                Token = data.access_token,
                ExpiresIn = TimeSpan.FromSeconds(data.expires_in),
                RefreshToken = data.refresh_token,
                IsSecondStepRequired = data.error_code == 15,
                TsaToken = data.tsa_token
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