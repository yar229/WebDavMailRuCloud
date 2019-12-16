using System;
using System.Collections.Generic;
using System.Linq;
//using YaR.MailRuCloud.Api.Base.Repos.WebV2.Requests;
using YaR.MailRuCloud.Api.Base.Requests.Types;
using YaR.MailRuCloud.Api.Links;

namespace YaR.MailRuCloud.Api.Base.Repos
{
    internal static class DtoImportWeb
    {
        private static readonly log4net.ILog Logger = log4net.LogManager.GetLogger(typeof(DtoImportWeb));

        public static UnpublishResult ToUnpublishResult(this CommonOperationResult<string> data)
        {
            var res = new UnpublishResult
            {
                IsSuccess = data.Status == 200
            };
            return res;
        }

        public static RenameResult ToRenameResult(this CommonOperationResult<string> data)
        {
            var res = new RenameResult
            {
                IsSuccess = data.Status == 200
            };
            return res;
        }


        public static RemoveResult ToRemoveResult(this CommonOperationResult<string> data)
        {
            var res = new RemoveResult
            {
                IsSuccess = data.Status == 200
            };
            return res;
        }

        public static PublishResult ToPublishResult(this CommonOperationResult<string> data)
        {
            var res = new PublishResult
            {
                IsSuccess = data.Status == 200,
                Url = data.Body
            };
            return res;
        }


        public static CopyResult ToCopyResult(this CommonOperationResult<string> data)
        {
            var res = new CopyResult
            {
                IsSuccess = data.Status == 200,
                NewName = data.Body
            };
            return res;
        }


        public static CloneItemResult ToCloneItemResult(this CommonOperationResult<string> data)
        {
            var res = new CloneItemResult
            {
                IsSuccess = data.Status == 200,
                Path = data.Body
            };
            return res;
        }


        public static AddFileResult ToAddFileResult(this CommonOperationResult<string> data)
        {
            var res = new AddFileResult
            {
                Success = data.Status == 200,
                Path = data.Body
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

        public static CreateFolderResult ToCreateFolderResult(this CommonOperationResult<string> data)
        {
            var res = new CreateFolderResult
            {
                IsSuccess = data.Status == 200,
                Path = data.Body
            };
            return res;
        }

        public static AccountInfoResult ToAccountInfo(this AccountInfoRequestResult data)
        {
            var res = new AccountInfoResult
            {
                FileSizeLimit = data.Body.Cloud.FileSizeLimit,

                DiskUsage = new DiskUsage
                {
                    Total = data.Body.Cloud.Space.BytesTotal, //total * 1024 * 1024,
                    Used = data.Body.Cloud.Space.BytesUsed, //used * 1024 * 1024,
                    OverQuota = data.Body.Cloud.Space.IsOverquota
                }
            };

            return res;
        }

        public static AuthTokenResult ToAuthTokenResult(this AuthTokenRequestResult data)
        {
            var res = new AuthTokenResult
            {
                IsSuccess = true,
                Token = data.Body.Token,
                ExpiresIn = TimeSpan.FromMinutes(58),
                RefreshToken = string.Empty
            };

            return res;
        }


        public static string ToToken(this DownloadTokenResult data)
        {
            var res = data.Body.Token;
            return res;
        }


        public static Dictionary<ShardType, ShardInfo> ToShardInfo(this ShardInfoRequestResult webdata)
        {
            var dict = new Dictionary<ShardType, ShardInfo>
            {
                {ShardType.Video,             new ShardInfo{Type = ShardType.Video,       Url = webdata.Body.Video[0].Url} },    
                {ShardType.ViewDirect,        new ShardInfo{Type = ShardType.ViewDirect,  Url = webdata.Body.ViewDirect[0].Url} },
                {ShardType.WeblinkView,       new ShardInfo{Type = ShardType.WeblinkView, Url = webdata.Body.WeblinkView[0].Url} },
                {ShardType.WeblinkVideo,      new ShardInfo{Type = ShardType.WeblinkVideo, Url = webdata.Body.WeblinkVideo[0].Url} },
                {ShardType.WeblinkGet,        new ShardInfo{Type = ShardType.WeblinkGet, Url = webdata.Body.WeblinkGet[0].Url} },
                {ShardType.WeblinkThumbnails, new ShardInfo{Type = ShardType.WeblinkThumbnails, Url = webdata.Body.WeblinkThumbnails[0].Url} },
                {ShardType.Auth,              new ShardInfo{Type = ShardType.Auth, Url = webdata.Body.Auth[0].Url} },
                {ShardType.View,              new ShardInfo{Type = ShardType.View, Url = webdata.Body.View[0].Url} },
                {ShardType.Get,               new ShardInfo{Type = ShardType.Get, Url = webdata.Body.Get[0].Url} },
                {ShardType.Upload,            new ShardInfo{Type = ShardType.Upload, Url = webdata.Body.Upload[0].Url} },
                {ShardType.Thumbnails,        new ShardInfo{Type = ShardType.Thumbnails, Url = webdata.Body.Thumbnails[0].Url} }
            };

            return dict;
        }

        private static readonly string[] FolderKinds = { "folder", "camera-upload", "mounted", "shared" };

        public static IEntry ToEntry(this FolderInfoResult data)
        {
            if (data.Body.Kind == "file")
            {
                var file = data.ToFile();
                return file;
            }

            var folder = new Folder(data.Body.Size, WebDavPath.Combine(data.Body.Home ?? WebDavPath.Root, data.Body.Name))
            {
                Folders = data.Body.List?
                    .Where(it => FolderKinds.Contains(it.Kind))
                    .Select(item => item.ToFolder())
                    .ToList(),
                Files = data.Body.List?
                    .Where(it => it.Kind == "file")
                    .Select(item => item.ToFile())
                    .ToGroupedFiles()
                    .ToList()
            };


            return folder;
        }


		//public static IEntry ToEntry(this FolderInfoResult data, Link ulink, string origPath)
		//{
		//	MailRuCloud.ItemType itemType;
		//	if (null == ulink)
		//		itemType = data.Body.Home == origPath
		//			? MailRuCloud.ItemType.Folder
		//			: MailRuCloud.ItemType.File;
		//	else
		//		itemType = ulink.ItemType;


		//	var entry = itemType == MailRuCloud.ItemType.File
		//		? (IEntry)data.ToFile(
		//			home: WebDavPath.Parent(origPath),
		//			ulink: ulink,
		//			filename: ulink == null ? WebDavPath.Name(origPath) : ulink.OriginalName,
		//			nameReplacement: WebDavPath.Name(origPath))
		//		: data.ToFolder(origPath, ulink);

		//	return entry;
		//}


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

            foreach (var propse in data.Body.List)
            {
                string name = link.OriginalName == propse.Name ? link.Name : propse.Name;
                propse.Home = WebDavPath.Combine(home, name);
                propse.Name = name;
            }
            data.Body.Home = home;
        }

        public static Folder ToFolder(this FolderInfoResult data, string home = null, Link link = null)
        {
            PatchEntryPath(data, home, link);

            var folder = new Folder(data.Body.Size, data.Body.Home ?? data.Body.Name, data.Body.Weblink)
            {
                Folders = data.Body.List?
                    .Where(it => FolderKinds.Contains(it.Kind))
                    .Select(item => item.ToFolder())
                    .ToList(),
                Files = data.Body.List?
                    .Where(it => it.Kind == "file")
                    .Select(item => item.ToFile())
                    .ToGroupedFiles()
                    .ToList(),
	            IsChildsLoaded = true
            };

            return folder;
        }

        //TODO: subject to heavily refact
        public static File ToFile(this FolderInfoResult data, string home = null, Link ulink = null, string filename = null, string nameReplacement = null)
        {
            if (ulink == null || ulink.IsLinkedToFileSystem)
                if (string.IsNullOrEmpty(filename))
                {
                    return new File(WebDavPath.Combine(data.Body.Home ?? "", data.Body.Name), data.Body.Size);
                }

            PatchEntryPath(data, home, ulink);

            var z = data.Body.List?
                .Where(it => it.Kind == "file")
                .Select(it => filename != null && it.Name == filename
                                ? it.ToFile(nameReplacement)
                                : it.ToFile())
                .ToList();

            var cmpname = string.IsNullOrEmpty(nameReplacement)
                ? filename
                : nameReplacement;

            if (string.IsNullOrEmpty(cmpname) && data.Body.Weblink != "/" && ulink != null && !ulink.IsLinkedToFileSystem)
            {
                cmpname = WebDavPath.Name(ulink.PublicLink);
            }

            var groupedFile = z?
                .ToGroupedFiles();


            var res = groupedFile.First(it => string.IsNullOrEmpty(cmpname) || it.Name == cmpname);

            return res;
        }

        private static Folder ToFolder(this FolderInfoResult.FolderInfoBody.FolderInfoProps item)
        {
            var folder = new Folder(item.Size, item.Home ?? item.Name, string.IsNullOrEmpty(item.Weblink) ? "" : item.Weblink);
            return folder;
        }

        private static File ToFile(this FolderInfoResult.FolderInfoBody.FolderInfoProps item, string nameReplacement = null)
        {
            try
            {

                var path = string.IsNullOrEmpty(nameReplacement)
                    ? item.Home
                    : WebDavPath.Combine(WebDavPath.Parent(item.Home), nameReplacement);

            
                var file = new File(path ?? item.Name, item.Size, item.Hash)
                {
                    PublicLink = string.IsNullOrEmpty(item.Weblink) ? string.Empty : item.Weblink
                };
                var dt = UnixTimeStampToDateTime(item.Mtime, file.CreationTimeUtc);
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
