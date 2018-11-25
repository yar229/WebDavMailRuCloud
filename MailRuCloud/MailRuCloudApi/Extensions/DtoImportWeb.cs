using System;
using System.Collections.Generic;
using System.Linq;
using YaR.MailRuCloud.Api.Base;
using YaR.MailRuCloud.Api.Base.Requests.Types;
using YaR.MailRuCloud.Api.Base.Requests.WebV2;
using YaR.MailRuCloud.Api.Links;

namespace YaR.MailRuCloud.Api.Extensions
{
    internal static class DtoImportWeb
    {
        private static readonly log4net.ILog Logger = log4net.LogManager.GetLogger(typeof(DtoImportWeb));

        public static UnpublishResult ToUnpublishResult(this UnpublishRequest.Result data)
        {
            var res = new UnpublishResult
            {
                IsSuccess = data.status == 200
            };
            return res;
        }

        public static RenameResult ToRenameResult(this RenameRequest.Result data)
        {
            var res = new RenameResult
            {
                IsSuccess = data.status == 200
            };
            return res;
        }


        public static RemoveResult ToRemoveResult(this RemoveRequest.Result data)
        {
            var res = new RemoveResult
            {
                IsSuccess = data.status == 200
            };
            return res;
        }

        public static PublishResult ToPublishResult(this PublishRequest.Result data)
        {
            var res = new PublishResult
            {
                IsSuccess = data.status == 200,
                Url = data.body
            };
            return res;
        }


        public static CopyResult ToCopyResult(this CopyRequest.Result data)
        {
            var res = new CopyResult
            {
                IsSuccess = data.status == 200,
                NewName = data.body
            };
            return res;
        }


        public static CloneItemResult ToCloneItemResult(this CloneItemRequest.Result data)
        {
            var res = new CloneItemResult
            {
                IsSuccess = data.status == 200,
                Path = data.body
            };
            return res;
        }


        public static AddFileResult ToAddFileResult(this CreateFileRequest.Result data)
        {
            var res = new AddFileResult
            {
                Success = data.status == 200,
                Path = data.body
            };
            return res;
        }


        public static UploadFileResult ToUploadPathResult(this string data)
        {
            var resp = data.Split(';');

            var res = new UploadFileResult
            {
                Hash = resp[0],
                Size = resp.Length > 1 
                    ? long.Parse(resp[1].Trim('\r', '\n', ' '))
                    : 0
            };
            return res;
        }

        public static CreateFolderResult ToCreateFolderResult(this CreateFolderRequest.Result data)
        {
            var res = new CreateFolderResult
            {
                IsSuccess = data.status == 200,
                Path = data.body
            };
            return res;
        }

        public static AccountInfoResult ToAccountInfo(this AccountInfoRequest.Result data)
        {
            var res = new AccountInfoResult
            {
                FileSizeLimit = data.body.cloud.file_size_limit,

                DiskUsage = new DiskUsage
                {
                    Total = data.body.cloud.space.bytes_total, //total * 1024 * 1024,
                    Used = data.body.cloud.space.bytes_used, //used * 1024 * 1024,
                    OverQuota = data.body.cloud.space.overquota
                }
            };

            return res;
        }

        public static AuthTokenResult ToAuthTokenResult(this AuthTokenRequest.Result data)
        {
            var res = new AuthTokenResult
            {
                IsSuccess = true,
                Token = data.body.token,
                ExpiresIn = TimeSpan.FromMinutes(58),
                RefreshToken = string.Empty
            };

            return res;
        }


        public static string ToToken(this DownloadTokenResult data)
        {
            var res = data.body.token;
            return res;
        }


        public static Dictionary<ShardType, ShardInfo> ToShardInfo(this ShardInfoRequest.Result webdata)
        {
            var dict = new Dictionary<ShardType, ShardInfo>
            {
                {ShardType.Video,             new ShardInfo{Type = ShardType.Video,       Url = webdata.body.video[0].url} },    
                {ShardType.ViewDirect,        new ShardInfo{Type = ShardType.ViewDirect,  Url = webdata.body.view_direct[0].url} },
                {ShardType.WeblinkView,       new ShardInfo{Type = ShardType.WeblinkView, Url = webdata.body.weblink_view[0].url} },
                {ShardType.WeblinkVideo,      new ShardInfo{Type = ShardType.WeblinkVideo, Url = webdata.body.weblink_video[0].url} },
                {ShardType.WeblinkGet,        new ShardInfo{Type = ShardType.WeblinkGet, Url = webdata.body.weblink_get[0].url} },
                {ShardType.WeblinkThumbnails, new ShardInfo{Type = ShardType.WeblinkThumbnails, Url = webdata.body.weblink_thumbnails[0].url} },
                {ShardType.Auth,              new ShardInfo{Type = ShardType.Auth, Url = webdata.body.auth[0].url} },
                {ShardType.View,              new ShardInfo{Type = ShardType.View, Url = webdata.body.view[0].url} },
                {ShardType.Get,               new ShardInfo{Type = ShardType.Get, Url = webdata.body.get[0].url} },
                {ShardType.Upload,            new ShardInfo{Type = ShardType.Upload, Url = webdata.body.upload[0].url} },
                {ShardType.Thumbnails,        new ShardInfo{Type = ShardType.Thumbnails, Url = webdata.body.thumbnails[0].url} }
            };

            return dict;
        }

        private static readonly string[] FolderKinds = { "folder", "camera-upload", "mounted", "shared" };

        public static IEntry ToEntry(this FolderInfoResult data)
        {
            if (data.body.kind == "file")
            {
                var file = data.ToFile();
                return file;
            }

            var folder = new Folder(data.body.size, WebDavPath.Combine(data.body.home ?? WebDavPath.Root, data.body.name))
            {
                Folders = data.body.list?
                    .Where(it => FolderKinds.Contains(it.kind))
                    .Select(item => item.ToFolder())
                    .ToList(),
                Files = data.body.list?
                    .Where(it => it.kind == "file")
                    .Select(item => item.ToFile())
                    .ToGroupedFiles()
                    .ToList()
            };


            return folder;
        }


		public static IEntry ToEntry(this FolderInfoResult data, Link ulink, string origPath)
		{
			MailRuCloud.ItemType itemType;
			if (null == ulink)
				itemType = data.body.home == origPath
					? MailRuCloud.ItemType.Folder
					: MailRuCloud.ItemType.File;
			else
				itemType = ulink.ItemType;


			var entry = itemType == MailRuCloud.ItemType.File
				? (IEntry)data.ToFile(
					home: WebDavPath.Parent(origPath),
					ulink: ulink,
					filename: ulink == null ? WebDavPath.Name(origPath) : ulink.OriginalName,
					nameReplacement: WebDavPath.Name(origPath))
				: data.ToFolder(origPath, ulink);

			return entry;
		}


		/// <summary>
		/// When it's a linked item, need to shift paths
		/// </summary>
		/// <param name="data"></param>
		/// <param name="home"></param>
		/// <param name="link"></param>
		private static void PatchEntryPath(FolderInfoResult data, string home, Link link)
        {
            if (string.IsNullOrEmpty(home) || null == link)
                return;

            foreach (var propse in data.body.list)
            {
                string name = link.OriginalName == propse.name ? link.Name : propse.name;
                propse.home = WebDavPath.Combine(home, name);
                propse.name = name;
            }
            data.body.home = home;
        }

        public static Folder ToFolder(this FolderInfoResult data, string home = null, Link link = null)
        {
            PatchEntryPath(data, home, link);

            var folder = new Folder(data.body.size, data.body.home ?? data.body.name, data.body.weblink)
            {
                Folders = data.body.list?
                    .Where(it => FolderKinds.Contains(it.kind))
                    .Select(item => item.ToFolder())
                    .ToList(),
                Files = data.body.list?
                    .Where(it => it.kind == "file")
                    .Select(item => item.ToFile())
                    .ToGroupedFiles()
                    .ToList(),
	            IsChildsLoaded = true
            };

            return folder;
        }

        public static File ToFile(this FolderInfoResult data, string home = null, Link ulink = null, string filename = null, string nameReplacement = null)
        {
            if (string.IsNullOrEmpty(filename))
            {
                return new File(WebDavPath.Combine(data.body.home ?? "", data.body.name), data.body.size);
            }

            PatchEntryPath(data, home, ulink);

            var z = data.body.list?
                .Where(it => it.kind == "file")
                .Select(it => filename != null && it.name == filename
                                ? it.ToFile(nameReplacement)
                                : it.ToFile())
                .ToList();
            var groupedFile = z?
                .ToGroupedFiles()
                .First(it => it.Name == (string.IsNullOrEmpty(nameReplacement) ? filename : nameReplacement));

            return groupedFile;
        }

        private static Folder ToFolder(this FolderInfoResult.FolderInfoBody.FolderInfoProps item)
        {
            var folder = new Folder(item.size, item.home ?? item.name, string.IsNullOrEmpty(item.weblink) ? "" : item.weblink);
            return folder;
        }

        private static File ToFile(this FolderInfoResult.FolderInfoBody.FolderInfoProps item, string nameReplacement = null)
        {
            try
            {

            var path = string.IsNullOrEmpty(nameReplacement)
                ? item.home
                : WebDavPath.Combine(WebDavPath.Parent(item.home), nameReplacement);

            
            var file = new File(path ?? item.name, item.size, item.hash)
            {
                PublicLink = string.IsNullOrEmpty(item.weblink) ? string.Empty : item.weblink
            };
            var dt = UnixTimeStampToDateTime(item.mtime, file.CreationTimeUtc);
            file.CreationTimeUtc =
                file.LastAccessTimeUtc =
                    file.LastWriteTimeUtc = dt;

            return file;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }

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

        private static DateTime UnixTimeStampToDateTime(double unixTimeStamp, DateTime defaultvalue)
        {
            try
            {
                // Unix timestamp is seconds past epoch
                var dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
                dtDateTime = dtDateTime.AddSeconds(unixTimeStamp); //.ToLocalTime(); - doesn't need, clients usially convert to localtime by itself
                return dtDateTime;
            }
            catch (Exception e)
            {
                Logger.Error($"Error converting unixTimeStamp {unixTimeStamp} to DateTime, {e.Message}");
                return defaultvalue;
            }
        }
    }
}
