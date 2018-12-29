using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Authentication;
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
		    var res = source.ToFile();//new File(data.FullPath, (long)source.Size, source.Sha1.ToHexString());
		    return res;
	    }

		public static YaR.MailRuCloud.Api.Base.File ToFile(this FsFile data)
		{
		    var res = new File(data.FullPath, (long) data.Size, data.Sha1.ToHexString())
		    {
                CreationTimeUtc = data.ModifDate,
                LastAccessTimeUtc = data.ModifDate,
                LastWriteTimeUtc = data.ModifDate //TODO: this is the main time (
            };
			return res;
		}

		private static IEnumerable<File> ToGroupedFiles(this IEnumerable<File> list)
		{
			var groupedFiles = list
				.GroupBy(f => f.ServiceInfo.CleanName,
					file => file)
				.SelectMany(group => group.Count() == 1         //TODO: DIRTY: if group contains header file, than make SplittedFile, else do not group
					? group.Take(1)
					: group.Any(f => f.Name == f.ServiceInfo.CleanName)
						? Enumerable.Repeat(new SplittedFile(group.ToList()), 1)
						: group.Select(file => file));

			return groupedFiles;
		}

		public static YaR.MailRuCloud.Api.Base.Folder ToFolder(this ListRequest.Result data)
	    {
		    var res = (data.Item as FsFolder)?.ToFolder();
		    return res;
	    }

		public static YaR.MailRuCloud.Api.Base.Folder ToFolder(this FsFolder data)
		{
			var res = new Folder((long)data.Size, data.FullPath) { IsChildsLoaded = data.IsChildsLoaded };

			res.Files.AddRange(data.Items
				.OfType<FsFile>()
				.Select(f => f.ToFile())
				.ToGroupedFiles());

			foreach (var it in data.Items.OfType<FsFolder>())
			{
				res.Folders.Add(it.ToFolder());
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
                throw new AuthenticationException($"OAuth: Error Code={data.error_code}, Value={data.error}, Description={data.error_description}");

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