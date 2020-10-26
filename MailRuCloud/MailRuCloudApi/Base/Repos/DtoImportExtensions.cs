using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Authentication;
using YaR.Clouds.Base.Repos.MailRuCloud;
using YaR.Clouds.Base.Repos.MailRuCloud.Mobile.Requests;
using YaR.Clouds.Base.Repos.MailRuCloud.Mobile.Requests.Types;
using YaR.Clouds.Base.Requests.Types;
using YaR.Clouds.Extensions;

namespace YaR.Clouds.Base.Repos
{
    internal static class DtoImportExtensions
    {
        internal static IEntry ToEntry(this ListRequest.Result data)
	    {
			Cloud.ItemType itemType = data.Item is FsFile ? Cloud.ItemType.File : Cloud.ItemType.Folder;

		    var entry = itemType == Cloud.ItemType.File
			    ? (IEntry)data.ToFile()
			    : data.ToFolder();

		    return entry;
		}

        internal static File ToFile(this ListRequest.Result data)
	    {
		    var source = data.Item as FsFile;
		    var res = source.ToFile();//new File(data.FullPath, (long)source.Size, source.Sha1.ToHexString());
		    return res;
	    }

        internal static File ToFile(this FsFile data)
		{
		    var res = new File(data.FullPath, (long) data.Size, new FileHashMrc(data.Sha1.ToHexString()))
		    {
                CreationTimeUtc = data.ModifDate,
                LastAccessTimeUtc = data.ModifDate,
                LastWriteTimeUtc = data.ModifDate //TODO: this is the main time (
            };
			return res;
		}

		internal static IEnumerable<File> ToGroupedFiles(this IEnumerable<File> list)
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

        internal static Folder ToFolder(this ListRequest.Result data)
	    {
		    var res = (data.Item as FsFolder)?.ToFolder();
		    return res;
	    }

        internal static Folder ToFolder(this FsFolder data)
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






        internal static CopyResult ToCopyResult(this MoveRequest.Result data, string newName)
        {
            var res = new CopyResult
            {
                IsSuccess = true,
                NewName = newName
            };
            return res;
        }

        internal static RenameResult ToRenameResult(this MoveRequest.Result data)
        {
            var res = new RenameResult
            {
                IsSuccess = true
            };
            return res;
        }

        internal static AddFileResult ToAddFileResult(this MobAddFileRequest.Result data)
        {
            var res = new AddFileResult
            {
                Success = data.IsSuccess,
                Path = data.Path
            };
            return res;
        }


        internal static AuthTokenResult ToAuthTokenResult(this OAuthRefreshRequest.Result data, string refreshToken)
        {
            if (data.ErrorCode > 0)
                throw new Exception($"OAuth: Error Code={data.ErrorCode}, Value={data.Error}, Description={data.ErrorDescription}");

            var res = new AuthTokenResult
            {
                IsSuccess = true,
                Token = data.AccessToken,
                ExpiresIn = TimeSpan.FromSeconds(data.ExpiresIn),
                RefreshToken = refreshToken
            };

            return res;
        }

        internal static AuthTokenResult ToAuthTokenResult(this OAuthRequest.Result data)
        {
            if (data.ErrorCode > 0 && data.ErrorCode != 15)
                throw new AuthenticationException($"OAuth: Error Code={data.ErrorCode}, Value={data.Error}, Description={data.ErrorDescription}");

            var res = new AuthTokenResult
            {
                IsSuccess = true,
                Token = data.AccessToken,
                ExpiresIn = TimeSpan.FromSeconds(data.ExpiresIn),
                RefreshToken = data.RefreshToken,
                IsSecondStepRequired = data.ErrorCode == 15,
                TsaToken = data.TsaToken
            };

            return res;
        }

        internal static CreateFolderResult ToCreateFolderResult(this CreateFolderRequest.Result data)
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